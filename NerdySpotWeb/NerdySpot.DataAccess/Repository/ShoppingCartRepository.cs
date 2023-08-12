using NerdySpot.DataAccess.Data;
using NerdySpot.DataAccess.Repository.IRepository;
using NerdySpot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NerdySpot.DataAccess.Repository
{
    //CategoryRepository class inherits from a base class
    //named Repository<Category> and implements the
    //ICategoryRepository
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {

        private readonly AppDbContext _db;

        //passing the db parameter to the base class
        //constructor allows the derived class
        //CategoryRepository to reuse and extend the data
        //access functionality provided by the base
        //class Repository<Category>.
        public ShoppingCartRepository(AppDbContext db) : base(db) //calling the constructor of base class
        {
            _db = db;
        }
        

        public void Update(ShoppingCart obj)
        {
            _db.ShoppingCarts.Update(obj);
        }
    }
}
