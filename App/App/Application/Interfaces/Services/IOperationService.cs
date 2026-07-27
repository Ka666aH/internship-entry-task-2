using App.Application.DTO;
using App.Application.Enums;
using App.Domain;

namespace App.Application.Interfaces.Services
{
    public interface IOperationService
    {
        Task<Operation?> CreateAsync(OperationCreateRequest request, CancellationToken ct = default);
        Task<SubmitResult> SubmitAsync(string operationId, CancellationToken ct = default);
        Task<Operation?> GetAsync(string operationId, CancellationToken ct = default);

        Task<List<OperationEvent>> GetOperationEventsListAsync(string operationId, CancellationToken ct = default);
    }
}
