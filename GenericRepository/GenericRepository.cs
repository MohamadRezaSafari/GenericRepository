using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Core3.Models;
using Dasync.Collections;
using Framework.Persistence.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Framework.Persistence.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly IUnitOfWork unitOfWork;
        private ClinicContext context;

        public GenericRepository(ClinicContext context)
        {
            this.context = context;
            unitOfWork = new UnitOfWork(context);
        }

        private static Func<ClinicContext, IAsyncEnumerable<T>> getCompileAsync =
            EF.CompileAsyncQuery<ClinicContext, T>((context)
                => context.Set<T>().AsNoTracking());

        private static Func<ClinicContext, IEnumerable<T>> getCompile =
            EF.CompileQuery<ClinicContext, T>((context)
                => context.Set<T>().AsNoTracking());

        public async Task<ICollection<T>> GetAllWithCompileQueryAsync()
        {
            return await getCompileAsync(context).ToListAsync();
        }

        public ICollection<T> GetAllWithCompileQuery()
        {
            return getCompile(context).ToList();
        }

        public IQueryable<T> Query()
        {
            return context.Set<T>().AsQueryable();
        }

        public ICollection<T> GetAll()
        {
            return context.Set<T>().ToList();
        }
        
        public async Task<ICollection<T>> GetAllAsync()
        {
            return await EntityFrameworkQueryableExtensions.ToListAsync(context.Set<T>());
        }

        public IEnumerable<T> ExecWithStoreProcedure(string query)
        {
            return context.Set<T>().FromSqlRaw(query).AsNoTracking().ToList<T>();
        }

        public async Task<IList<T>> ExecWithStoreProcedureAsync(string query)
        {
            return await context.Set<T>().FromSqlRaw(query).AsNoTracking().ToListAsync<T>();
        }

        public async Task<IList<T>> ExecWithStoreProcedureAsync(string query, params object[] parameters)
        {
            return await context.Set<T>().FromSqlRaw(query, parameters).ToListAsync();
        }
        
        public T GetById(int? id)
        {
            if (id == null)
            {
                throw new ArgumentException("null");
            }
            return context.Set<T>().Find(id);
        }

        public async Task<T> GetByIdAsync(int? id)
        {
            if (id == null)
            {
                throw new ArgumentException("null");
            }
            return await context.Set<T>().FindAsync(id);
        }

        public T GetByUniqueId(string id)
        {
            return context.Set<T>().Find(id);
        }

        public async Task<T> GetByUniqueIdAsync(string id)
        {
            return await context.Set<T>().FindAsync(id);
        }

        public T Find(Expression<Func<T, bool>> match)
        {
            return context.Set<T>().SingleOrDefault(match);
        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> match)
        {
            return await context.Set<T>().SingleOrDefaultAsync(match);
        }

        public ICollection<T> FindAll(Expression<Func<T, bool>> match)
        {
            return context.Set<T>().Where(match).ToList();
        }

        public async Task<ICollection<T>> FindAllAsync(Expression<Func<T, bool>> match)
        {
            return await context.Set<T>().Where(match).ToListAsync();
        }

        public T Add(T entity)
        {
            context.Set<T>().Add(entity);
            context.SaveChanges();
            return entity;
        }

        public async Task<T> AddAsync(T entity)
        {
            context.Set<T>().Add(entity);
            await unitOfWork.Commit();
            return entity;
        }

        public T Update(T updated)
        {
            if (updated == null)
            {
                return null;
            }

            context.Set<T>().Attach(updated);
            context.Entry(updated).State = EntityState.Modified;
            context.SaveChanges();

            return updated;
        }

        public async Task<T> UpdateAsync(T updated)
        {
            if (updated == null)
            {
                return null;
            }

            context.Set<T>().Attach(updated);
            context.Entry(updated).State = EntityState.Modified;
            await unitOfWork.Commit();

            return updated;
        }

        public void Delete(T t)
        {
            context.Set<T>().Remove(t);
            context.SaveChanges();
        }

        public async Task<int> DeleteAsync(T t)
        {
            context.Set<T>().Remove(t);
            return await unitOfWork.Commit();
        }

        public int Count()
        {
            return context.Set<T>().Count();
        }

        public async Task<int> CountAsync()
        {
            return await context.Set<T>().CountAsync();
        }

        public IEnumerable<T> Filter(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = "", int? page = null,
            int? pageSize = null)
        {
            IQueryable<T> query = context.Set<T>();
            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (includeProperties != null)
            {
                foreach (
                    var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            if (page != null && pageSize != null)
            {
                query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }

            return query.ToList();
        }

        public IQueryable<T> FindBy(Expression<Func<T, bool>> predicate)
        {
            return context.Set<T>().Where(predicate);
        }

        public bool Exist(Expression<Func<T, bool>> predicate)
        {
            var exist = context.Set<T>().Where(predicate);
            return exist.Any() ? true : false;
        }
    }
}
