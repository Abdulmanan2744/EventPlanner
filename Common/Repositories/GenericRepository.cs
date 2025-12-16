// Common/Repositories/GenericRepository.cs
using Common.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace Common.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class, new()
    {
        private readonly EventPlannerDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(EventPlannerDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public IEnumerable<T> GetAll()
        {
            try
            {
                return _dbSet.ToList();
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"An error occurred while fetching all records.", ex);
            }
        }

        public T GetByName(string name)
        {
            try
            {
                // Try to find Email property first, then Name
                var emailProperty = typeof(T).GetProperty("Email");
                var nameProperty = typeof(T).GetProperty("Name");

                if (emailProperty != null)
                {
                    return _dbSet.AsEnumerable()
                        .FirstOrDefault(e => emailProperty.GetValue(e)?.ToString()?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);
                }
                else if (nameProperty != null)
                {
                    return _dbSet.AsEnumerable()
                        .FirstOrDefault(e => nameProperty.GetValue(e)?.ToString()?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"An error occurred while fetching record by name.", ex);
            }
        }

        public T[] GetById(int id, bool isPrimaryKey, string columnName)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid ID provided.", nameof(id));
            }

            try
            {
                if (isPrimaryKey)
                {
                    var entity = _dbSet.Find(id);
                    return entity != null ? new[] { entity } : Array.Empty<T>();
                }
                else
                {
                    var property = typeof(T).GetProperty(columnName);
                    if (property == null)
                    {
                        throw new ArgumentException($"Property '{columnName}' not found on type {typeof(T).Name}");
                    }

                    return _dbSet.AsEnumerable()
                        .Where(e => Convert.ToInt32(property.GetValue(e)) == id)
                        .ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"An error occurred while fetching records by ID.", ex);
            }
        }

        public void Add(T entity)
        {
            try
            {
                _dbSet.Add(entity);
                // Note: Commit() will be called by UnitOfWork
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"An error occurred while adding a new record.", ex);
            }
        }

        public bool Update(T entity)
        {
            try
            {
                _dbSet.Update(entity);
                // Note: SaveChanges() will be called by UnitOfWork.Commit()
                return true;
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"An error occurred while updating a record.", ex);
            }
        }

        public bool Delete(int id)
        {
            try
            {
                var entity = _dbSet.Find(id);
                if (entity == null)
                {
                    return false;
                }

                _dbSet.Remove(entity);
                // Note: SaveChanges() will be called by UnitOfWork.Commit()
                return true;
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"An error occurred while deleting record with ID {id}.", ex);
            }
        }

        public IEnumerable<T> ExecuteQuery(string sql, object? parameters = null)
        {
            try
            {
                // For Entity Framework Core, use FromSqlRaw
                return _dbSet.FromSqlRaw(sql).ToList();
            }
            catch (Exception ex)
            {
                throw new RepositoryException("An error occurred while executing the query.", ex);
            }
        }
    }

    // Custom exception class for repository-related exceptions
    public class RepositoryException : Exception
    {
        public RepositoryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}