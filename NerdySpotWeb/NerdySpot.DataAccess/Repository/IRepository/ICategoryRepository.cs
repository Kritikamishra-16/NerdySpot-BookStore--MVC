using NerdySpot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerdySpot.DataAccess.Repository.IRepository
{
    //Here we are defining that when we need the implementation
    //for ICategoryRepository then we want this to get 
    //the base functionality of IRepository interface also 
    public interface ICategoryRepository : IRepository<Category>
    {
        void Update(Category obj);
    }
}
