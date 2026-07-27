using App.Application.DTO;
using App.Application.Enums;
using App.Application.Interfaces.Repositories;
using App.Application.Interfaces.Services;
using App.Domain;
using App.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace App.Application.Services
{
    public class OperationService : IOperationService
    {
        private readonly IOperationRepository _operationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public OperationService(IOperationRepository operationRepository, IUnitOfWork unitOfWork)
        {
            _operationRepository = operationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation?> CreateAsync(OperationCreateRequest request, CancellationToken ct = default)
        {
            Operation operation = new(request.OperationId, request.Amount, request.Currency, request.Description);
            await _operationRepository.CreateAsync(operation, ct);
            try
            {
                await _unitOfWork.SaveChangesAsync(ct);
                return operation;
            }
            catch(DbUpdateException)
            {
                return null;
            }
        }

        public async Task<Operation?> GetAsync(string operationId, CancellationToken ct = default) =>
            await _operationRepository.GetAsNoTrackingAsync(operationId);

        public async Task<SubmitResult> SubmitAsync(string operationId, CancellationToken ct = default)
        {
            await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
            Operation? operation = await _operationRepository.GetWithLockAsync(operationId, ct);

            if (operation == null) return SubmitResult.NotFound;
            if (operation.Status != OperationStatus.Created) return SubmitResult.Submitted;

            operation.Submit();
            await _unitOfWork.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return SubmitResult.Success;
        }
    }
}
