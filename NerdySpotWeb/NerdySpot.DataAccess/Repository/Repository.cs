using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using NerdySpot.DataAccess.Data;
using NerdySpot.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace NerdySpot.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _db;

        //this is done bcz '_db.T.Add(entity)' was invalid syntax
        internal DbSet<T> dbSet;
        public Repository(AppDbContext db)
        {
            _db = db;

            //1._db.Set<T>() is a method provided by the DbContext class. It returns a DbSet<T> object that represents a collection of entities of type T.
            //2.the line this.dbSet = _db.Set<T>(); is setting the dbSet property to a DbSet<T> object that represents the collection of entities of type T within the context of the _db DbContext.

            this.dbSet = _db.Set<T>();
            //So this is equivalent after the above line
            //_db.Categories == dbSet

            /*INCLUDE:-
             * Populating(Including) the Navigation property based on the foreign 
             key relation and it is provided by EF core

            _db.Products.Include(u => u.Category).Include(u=>u.CoverType);
            */
        }
        public void Add(T entity) 
        {
            dbSet.Add(entity);
        }


        /*
        Expression<Func<T, bool>>:

        The Expression is a class in the System.Linq.Expressions namespace that represents an expression tree, which allows representing code as data.
        Func<T, bool> is a delegate type representing a function that takes an input of type T and returns a boolean value (true or false).
        The combination of Expression<Func<T, bool>> represents a lambda expression or predicate that can be used as a filter condition to select elements of type T.
        */
        public T Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
        {

            /*The use of IQueryable<T> in the previous code is related to deferred execution and query optimization in LINQ.
               Deferred Execution:
                    IQueryable<T> provides deferred execution, which means the LINQ query is not executed immediately when the query is defined.
                    Instead, the query is executed when the result is actually needed, for example, when calling methods like FirstOrDefault(), ToList(), or when iterating over the query with a foreach loop.
                Query Optimization:
                    <T> allows LINQ providers(like Entity Framework for databases) to optimize the query before executing it against the data source.
            */

            IQueryable<T> query;
            if (tracked)
            {
                query = dbSet;
            }
            else
            {
                query = dbSet.AsNoTracking(); //this will make sure that the EF will not track the entity which is being retrived      
            }
            query = query.Where(filter);

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            return query.FirstOrDefault();


            /*In most cases, if you are only fetching all the data without any additional filtering or transformations, the second approach(return dbSet.ToList();) is simpler and efficient since it avoids the unnecessary assignment and deferred execution overhead of the first approach.
            However, if you need to apply additional filtering or query operations before fetching the data, the first approach(IQueryable<T> query = dbSet; return query.ToList();) allows you to compose a more flexible and optimized query.
            */

            //return dbSet.Where(filter).FirstOrDefault();
        }

        //Category,CoverType
        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter,string? includeProperties=null)
        {
            IQueryable<T> query = dbSet;
            if(filter != null)
            {
                query = query.Where(filter);
            }
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach(var includeProp in includeProperties
                    .Split(new char[] {','},StringSplitOptions.RemoveEmptyEntries))
                {
                    query=query.Include(includeProp);
                }
            }
            return query.ToList();


        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }
    }
}
