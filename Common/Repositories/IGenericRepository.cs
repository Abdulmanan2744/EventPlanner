using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T GetByName(string name);
        T[] GetById(int id, bool isPrimaryKey, string columnName);

        void Add(T entity);
        bool Update(T entity);
        bool Delete(int id);
        IEnumerable<T> ExecuteQuery(string sql, object? parameters = null);
    }
}
