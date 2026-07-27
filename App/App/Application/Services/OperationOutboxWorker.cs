using App.Application.Interfaces.Repositories;
using App.Domain;
using App.Domain.Enums;
using System.Collections.Concurrent;

namespace App.Application.Services
{
    public class OperationOutboxWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ConcurrentDictionary<string, OperationBackoff> _backoff = new();
        private static readonly TimeSpan BaseDelay = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(60);
        private const int MaxDegreeOfParallelism = 5;
        public OperationOutboxWorker(IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory)
        {
            _scopeFactory = scopeFactory;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                List<Operation> operations;

                using (var scope = _scopeFactory.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IOperationRepository>();
                    operations = await repo.GetByStatusAsync(OperationStatus.Processing, stoppingToken);
                }

                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = MaxDegreeOfParallelism,
                    CancellationToken = stoppingToken
                };

                await Parallel.ForEachAsync(operations, parallelOptions, async (op, ct) =>
                {
                    if (_backoff.TryGetValue(op.OperationId, out var state) && state.NextAttemptAt > DateTime.UtcNow) return;

                    using var scope = _scopeFactory.CreateScope();
                    var operationRepository = scope.ServiceProvider.GetRequiredService<IOperationRepository>();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var client = _httpClientFactory.CreateClient("Provider");

                    var request = new HttpRequestMessage(HttpMethod.Post, "/payments")
                    {
                        Content = JsonContent.Create(new { op.OperationId, op.Amount, op.Currency })
                    };
                    request.Headers.Add("Idempotency-Key", op.OperationId);
                    request.Headers.Add("X-Correlation-ID", op.OperationId);

                    try
                    {
                        var response = await client.SendAsync(request, ct);
                        if (response.IsSuccessStatusCode)
                        {
                            var result = await response.Content.ReadFromJsonAsync<ProviderResponse>(cancellationToken: ct);
                            await SavePaymentId(operationRepository, unitOfWork, op.OperationId, result!.ProviderPaymentId, ct);
                            _backoff.TryRemove(op.OperationId, out _);
                            return;
                        }
                    }
                    catch { }

                    var retryCount = state?.RetryCount ?? 0;
                    Backoff(op.OperationId, retryCount + 1);
                });
            }
        }
        private void Backoff(string operationId, int retryCount)
        {
            var exponential = BaseDelay * Math.Pow(2, retryCount);
            var clamped = TimeSpan.FromTicks(Math.Min(exponential.Ticks, MaxDelay.Ticks));
            var jitterMultiplier = 1.0 + Random.Shared.NextDouble() * 0.1;
            var delay = TimeSpan.FromTicks((long)(clamped.Ticks * jitterMultiplier));
            _backoff[operationId] = new OperationBackoff(retryCount, DateTime.UtcNow + delay);
        }
        private static async Task SavePaymentId(
            IOperationRepository operationRepository,
            IUnitOfWork unitOfWork,
            string operationId,
            Guid providerPaymentId,
            CancellationToken ct)
        {
            await using var tx = await unitOfWork.BeginTransactionAsync(ct);
            var operation = await operationRepository.GetWithLockAsync(operationId, ct);
            if (operation is null) return;
            try { operation.SetProviderPaymentId(providerPaymentId); }
            catch (InvalidOperationException) { return; }
            await unitOfWork.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        private record OperationBackoff(int RetryCount, DateTime NextAttemptAt);
    }
}
