using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_API.Contracts
{
    public interface IRepositoryBase<T> where T : class
    {   // Task is needed for asynchronous function
        Task<IList<T>> FindAll();//Task<FORM OF DATA BEING RETURNED> Find All.
        Task<T> FindById(int id);
        Task<bool> isExist(int id);
        Task<bool> Create(T entity);
        Task<bool> Update(T entity);
        Task<bool> Delete(T entity);
        Task<bool> Save();

        //remember this is BASE repo.
    }
}
