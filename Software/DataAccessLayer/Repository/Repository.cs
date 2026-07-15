using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly PassNestDbContext context;
        private readonly DbSet<T> dbSet;

        public Repository(PassNestDbContext context)
        {
            this.context = context;
            dbSet = context.Set<T>();
        }

        public T? GetById(int id) => dbSet.Find(id);
        public IEnumerable<T> GetAll(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = dbSet;
            foreach(var property in includeProperties)
            {
                query = query.Include(property);
            }

            return query.ToList();
        }
        public void Add(T entity) => dbSet.Add(entity);
        public void Update(T entity) => dbSet.Update(entity);
        public void Delete(T entity) => dbSet.Remove(entity);
        public void SaveChanges() => context.SaveChanges();
    }
}
