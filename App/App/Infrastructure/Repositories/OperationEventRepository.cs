using App.Application.Interfaces.Repositories;
using App.Domain;
using App.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories
{
    public class OperationEventRepository : IOperationEventRepository
    {
        private readonly AppDbContext _db;

        public OperationEventRepository(AppDbContext db) => _db = db;

        public async Task CreateAsync(OperationEvent operationEvent, CancellationToken ct = default) =>
            await _db.OperationEvents.AddAsync(operationEvent, ct);

        public async Task<List<OperationEvent>> GetListAsync(string operationId, CancellationToken ct = default) =>
            await _db.OperationEvents.AsNoTracking().Where(oe => oe.OperationId == operationId).ToListAsync(ct);
    }
}
