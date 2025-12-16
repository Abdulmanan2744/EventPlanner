// Common/UnitOfWork/UnitOfWork.cs
using Common.Data;
using Common.Repositories;

namespace Common.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EventPlannerDbContext _context;
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(EventPlannerDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<T> GetRepository<T>() where T : class, new()
        {
            var type = typeof(T);
            if (!_repositories.ContainsKey(type))
            {
                _repositories[type] = new GenericRepository<T>(_context);
            }
            return (IGenericRepository<T>)_repositories[type];
        }

        public void Commit()
        {
            _context.SaveChanges();
        }

        public void Rollback()
        {
            // Entity Framework handles rollback automatically
            // You can dispose and recreate context if needed
        }

        public void BeginTransaction()
        {
            _context.Database.BeginTransaction();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}