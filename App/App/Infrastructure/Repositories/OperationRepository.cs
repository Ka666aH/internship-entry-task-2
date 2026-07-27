using App.Application.Interfaces.Repositories;
using App.Domain;
using App.Domain.Enums;
using App.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories
{
    public class OperationRepository : IOperationRepository
    {
        private readonly AppDbContext _db;
        public OperationRepository(AppDbContext db) => _db = db;

        public async Task CreateAsync(Operation operation, CancellationToken ct = default) =>
            await _db.Operations
            .AddAsync(operation, ct);

        public async Task<Operation?> GetWithLockAsync(string operationId, CancellationToken ct = default)
        {
            return await _db.Operations
                .FromSqlRaw(
                """
                SELECT * FROM "Operations"
                WHERE "OperationId" = {0} FOR UPDATE
                """,
                operationId)
                .SingleOrDefaultAsync(ct);
        }

        public async Task<Operation?> GetAsNoTrackingAsync(string operationId, CancellationToken ct = default) =>
            await _db.Operations
            .AsNoTracking()
            .SingleOrDefaultAsync(o => o.OperationId == operationId, ct);

        public async Task<List<Operation>> GetByStatusAsync(OperationStatus status, CancellationToken ct = default)
        {
            return await _db.Operations
                .Where(o => o.Status == status)
                .ToListAsync(ct);
        }
    }
}
