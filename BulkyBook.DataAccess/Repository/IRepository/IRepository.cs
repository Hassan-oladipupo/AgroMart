using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        // getting the method we need for category class

        //for the GetFirstOrDefault method in the category
        T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = true);

        //for the Get method in the category
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter=null, string? includeProperties = null);

        //for the add method in the category
        void Add(T entity);

        //for the remove method in the category
        void Remove(T entity);
        //for the remove multiple entity method in the category
        void RemoveRange(IEnumerable<T> entity);  
        
    }


}
