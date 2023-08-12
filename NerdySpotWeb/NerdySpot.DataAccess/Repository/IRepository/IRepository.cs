using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NerdySpot.DataAccess.Repository.IRepository
{
    //GENERIC REPOSITORY
    public interface IRepository<T> where T : class
    {
        //T- Category or any other generic model on which
        //we want to perform the CRUD operation or rather want to 
        //interact whith the DbContext


        //IEnumerable<T> is a generic interface in
        //C# (and other .NET languages) that represents a
        //read-only sequence or collection
        //of elements of type T, interface provides methods
        //to iterate over the elements in the collection.
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter=null,string? includeProperties = null);

        T Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked=false);

        void Add(T entity);
        void Remove(T entity);

        void RemoveRange(IEnumerable<T> entities);



    }
}
