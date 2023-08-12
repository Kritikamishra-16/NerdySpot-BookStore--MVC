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
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        //Now we need to implement all the functionality of base IRepository Interface also bcz IProductRepository incudes that 
        //hence instead of defining all the functions of base IRepository Interface we are inheriting the base Repoclass.

        private readonly AppDbContext _db;
        public ProductRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Product obj)
        {
            Product productObjfromDb = _db.Products.FirstOrDefault(u => u.Id == obj.Id);
            if(productObjfromDb != null)
            {
                productObjfromDb.Title = obj.Title;
                productObjfromDb.Description = obj.Description;
                productObjfromDb.ISBN = obj.ISBN;
                productObjfromDb.Author= obj.Author;
                productObjfromDb.ListPrice = obj.ListPrice;
                productObjfromDb.Price = obj.Price;
                productObjfromDb.Price50 = obj.Price50;
                productObjfromDb.Price100 = obj.Price100;
                productObjfromDb.CategoryId= obj.CategoryId;
                if(obj.ImageUrl!=null)
                {
                    //only upadte existing image only if admin has entered the new image file
                    productObjfromDb.ImageUrl = obj.ImageUrl;
                }

            }
        }
    }
}
