using App.Application.Interfaces.Repositories;
using App.Application.Interfaces.Services;
using App.Domain;
using App.Domain.Enums;

namespace App.Application.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly IOperationRepository _operationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ReceiptService(IOperationRepository operationRepository, IUnitOfWork unitOfWork)
        {
            _operationRepository = operationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ReceiptResult> ProcessAsync(ReceiptRequest request, CancellationToken ct = default)
        {
            await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
            Operation? operation = await _operationRepository.GetWithLockAsync(request.OperationId, ct);

            if (operation == null) return ReceiptResult.NotFound;
            if (operation.ProviderPaymentId != null && operation.ProviderPaymentId != request.ProviderPaymentId) return ReceiptResult.Conflict;
            if (operation.Status != OperationStatus.Processing) return ReceiptResult.Processed;

            operation.SetProviderPaymentId(request.ProviderPaymentId);
            if (request.Result == "COMPLETED") operation.Complete();
            else operation.Reject();
            await _unitOfWork.SaveChangesAsync(ct);

            await tx.CommitAsync(ct);
            return ReceiptResult.Success;
        }
    }
}
