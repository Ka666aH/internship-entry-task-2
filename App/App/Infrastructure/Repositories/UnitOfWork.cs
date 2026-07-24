using App.Application.Interfaces.Repositories;
using App.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Storage;

namespace App.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _db;
        public UnitOfWork(AppDbContext db) => _db = db;

        public async Task SaveChangesAsync(CancellationToken ct = default) =>
            await _db.SaveChangesAsync(ct);
        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default) =>
            await _db.Database.BeginTransactionAsync(ct);
    }
}
