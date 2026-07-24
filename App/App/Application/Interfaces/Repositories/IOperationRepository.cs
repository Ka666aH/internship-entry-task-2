using App.Domain;

namespace App.Application.Interfaces.Repositories
{
    public interface IOperationRepository
    {
        Task CreateAsync(Operation operation, CancellationToken ct = default);
        Task<Operation?> GetWithLockAsync(string id, CancellationToken ct = default);
        Task<Operation?> GetAsNoTrackingAsync(string id, CancellationToken ct = default);
    }
}
