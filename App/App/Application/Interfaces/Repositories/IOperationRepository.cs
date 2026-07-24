using App.Domain;

namespace App.Application.Interfaces.Repositories
{
    public interface IOperationRepository
    {
        Task CreateAsync(Operation operation, CancellationToken ct = default);
        Task<Operation?> GetWithLockAsync(string operationId, CancellationToken ct = default);
        Task<Operation?> GetAsNoTrackingAsync(string operationId, CancellationToken ct = default);
    }
}
