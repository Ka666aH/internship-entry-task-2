using Microsoft.EntityFrameworkCore.Storage;

namespace App.Application.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken ct = default);
        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
    }
}
