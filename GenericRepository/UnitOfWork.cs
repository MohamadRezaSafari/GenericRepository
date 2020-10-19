﻿using System;
using System.Collections.Generic;
 using System.Linq;
using System.Threading.Tasks;
 using Core3.Models;
 using Framework.Persistence.Contracts;

 namespace Framework.Persistence.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ClinicContext _dbContext;
        private readonly ClinicContext dbContext;
        private readonly Dictionary<Type, object> _repositories = new Dictionary<Type, object>();

        public Dictionary<Type, object> Repositories
        {
            get => _repositories;
            set => Repositories = value;
        }
        
        public UnitOfWork(ClinicContext dbContext)
        {
            //this.dbContext = GetObjectContextFromEntity(dbContext);

            _dbContext = dbContext;
            //dbContext = dbContext.Set<ClinicContext>();
        }
        
        //public UnitOfWork(ClinicContext dbContext, DbContext context)
        //{
        //    _dbContext = dbContext;
        //}

        public IGenericRepository<T> Repository<T>() where T : class
        {
            if (Repositories.Keys.Contains(typeof(T)))
            {
                return Repositories[typeof(T)] as IGenericRepository<T>;
            }

            IGenericRepository<T> repo = new GenericRepository<T>(_dbContext);
            //IGenericRepository<T> identity = new GenericRepository<T>(applicationDbContext);
            Repositories.Add(typeof(T), repo);
            return repo;
        }

        public async Task<int> Commit()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public void Rollback()
        {
            _dbContext.ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
        }
    }
}
