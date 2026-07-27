using App.Domain;

namespace App.Application.Interfaces.Repositories
{
    public interface IOperationEventRepository
    {
        Task CreateAsync(OperationEvent operationEvent, CancellationToken ct = default);
        Task<List<OperationEvent>> GetListAsync(string operationId, CancellationToken ct = default);
    }
}
