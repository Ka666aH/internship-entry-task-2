using App.Application.DTO;
using App.Application.Enums;
using App.Application.Interfaces.Repositories;
using App.Application.Interfaces.Services;
using App.Domain;
using App.Domain.Enums;

namespace App.Application.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly IOperationRepository _operationRepository;
        private readonly IOperationEventRepository _operationEventRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ReceiptService(IOperationRepository operationRepository, IOperationEventRepository operationEventRepository, IUnitOfWork unitOfWork)
        {
            _operationRepository = operationRepository;
            _operationEventRepository = operationEventRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ReceiptResult> ProcessAsync(ReceiptRequest request, CancellationToken ct = default)
        {
            await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
            Operation? operation = await _operationRepository.GetWithLockAsync(request.OperationId, ct);

            if (operation == null) return ReceiptResult.NotFound;
            if (operation.Status != OperationStatus.Processing) return ReceiptResult.Processed;
            if (operation.ProviderPaymentId != null && operation.ProviderPaymentId != request.ProviderPaymentId) return ReceiptResult.Conflict;

            if (operation.ProviderPaymentId == null)
                operation.SetProviderPaymentId(request.ProviderPaymentId);
            if (request.Result == "COMPLETED")
            {
                operation.Complete();
                await _operationEventRepository.CreateAsync(new(3, request.OperationId, OperationStatus.Processing, OperationStatus.Completed, "Operation completed"), ct);
            }
            else
            {
                operation.Reject();
                await _operationEventRepository.CreateAsync(new(3, request.OperationId, OperationStatus.Processing, OperationStatus.Rejected, "Operation rejected"), ct);
            }
            await _unitOfWork.SaveChangesAsync(ct);

            await tx.CommitAsync(ct);
            return ReceiptResult.Success;
        }
    }
}
