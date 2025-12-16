using Common.Repositories;
using Common.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services
{
    public class GenericService<T> : IGenericService<T> where T : class, new()
    {
        private readonly IUnitOfWork _unitOfWork;

        public GenericService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public T GetByName(string name)
        {
            return _unitOfWork.GetRepository<T>().GetByName(name);
        }

        public IEnumerable<T> GetAll()
        {
            return _unitOfWork.GetRepository<T>().GetAll();
        }

        public T[] GetById(int id, bool isPrimaryKey, string columnName)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Invalid ID provided.", nameof(id));
            }
            if (isPrimaryKey)
            {
                var entity = _unitOfWork.GetRepository<T>().GetById(id, true, columnName);
                if (entity == null)
                {
                    // Handle case where no entity is found
                    throw new Exception($"Entity of type {typeof(T).Name} with ID {id} not found.");
                }

                return entity;
            }
            else
            {
                var entity = _unitOfWork.GetRepository<T>().GetById(id, false, columnName);
                if (entity == null)
                {
                    // Handle case where no entity is found
                    throw new Exception($"Entity of type {typeof(T).Name} with ID {id} not found.");
                }

                return entity;
            }
        }



        public void Add(T entity)
        {
            _unitOfWork.GetRepository<T>().Add(entity);
            _unitOfWork.Commit();

        }

        public bool Update(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "Entity to update cannot be null.");
            }

            try
            {
                var repository = _unitOfWork.GetRepository<T>();
                bool updateSuccess = repository.Update(entity);

                if (updateSuccess)
                {
                    _unitOfWork.Commit();
                    return true; // Update was successful
                }

                return false; // Update failed
            }
            catch (Exception)
            {
                // Optionally log the error here
                return false; // Handle failure
            }
        }

        /*  public void Update(T entity)
          {
              _unitOfWork.GetRepository<T>().Update(entity);
              _unitOfWork.Commit();
          }*/

        public bool Delete(int id)
        {
            try
            {
                var result = _unitOfWork.GetRepository<T>().Delete(id);
                _unitOfWork.Commit();
                if (result == true)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                return false;
                throw new RepositoryException($"An error occurred while deleting the record with ID {id}.", ex);
            }
        }


        public T? Authenticate(string username, string password) // Optional for specific use cases
        {
            // Implement authentication logic based on the entity type if needed
            // return null; // Return the authenticated entity or null

            var user = _unitOfWork.GetRepository<T>().GetByName(username);
            if (user == null)
            {
                return null;
            }
            return user;
        }
    }
}
