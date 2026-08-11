using DataAccessLayer.Repository;
using System.Linq.Expressions;

namespace BusinessLogicLayer.Tests
{
    public class InMemoryRepository<T> : IRepository<T> where T : class
    {
        private readonly List<T> items = new();
        private readonly Func<T, int> getId;
        private readonly Action<T, int> setId;
        private int nextId = 1;

        public InMemoryRepository(Func<T, int> getId, Action<T, int> setId)
        {
            this.getId = getId;
            this.setId = setId;
        }

        public IReadOnlyList<T> Items => items;

        public void Add(T entity)
        {
            setId(entity, nextId++);
            items.Add(entity);
        }

        public void Delete(T entity) => items.Remove(entity);

        public IEnumerable<T> GetAll(params Expression<Func<T, object>>[] includeProperties) => items;

        public T? GetById(int id) => items.FirstOrDefault(i => getId(i) == id);

        public void SaveChanges() { }

        public void Update(T entity) { }

        public void Clear() => items.Clear();
    }
}