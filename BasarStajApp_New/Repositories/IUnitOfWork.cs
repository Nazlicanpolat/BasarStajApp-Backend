using System;
using System.Threading.Tasks;
using BasarStajApp_New.Repositories;

namespace BasarStajApp_New.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;
        Task<int> CommitAsync();
    }
}
