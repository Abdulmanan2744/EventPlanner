using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services
{
    public interface IGenericService<T>
    {
        IEnumerable<T> GetAll();
        T GetByName(string name);
        T[] GetById(int id, bool isPrimaryKey, string columnName);

        void Add(T entity);
        bool Update(T entity);
        bool Delete(int id);
        T Authenticate(string username, string password); // Optional for specific use cases

    }
}
