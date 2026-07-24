using App.Application.Interfaces.Repositories;
using App.Domain;
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

        public async Task<Operation?> GetWithLockAsync(string id, CancellationToken ct = default)
        {
            return await _db.Operations
                .FromSqlRaw(
                """
                SELECT * FROM "Operations"
                WHERE "OperationId" = {0} FOR UPDATE NOWAIT
                """,
                id)
                .SingleOrDefaultAsync(ct);
        }

        public async Task<Operation?> GetAsNoTrackingAsync(string id, CancellationToken ct = default) =>
            await _db.Operations
            .AsNoTracking()
            .SingleOrDefaultAsync(o => o.OperationId == id, ct);
    }
}
