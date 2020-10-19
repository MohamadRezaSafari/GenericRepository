using System.Threading.Tasks;
using Framework.Persistence;

namespace Framework.Persistence.Contracts
{
    public interface IUnitOfWork
    {
        IGenericRepository<T> Repository<T>() where T : class;

        Task<int> Commit();

        void Rollback();
    }
}
