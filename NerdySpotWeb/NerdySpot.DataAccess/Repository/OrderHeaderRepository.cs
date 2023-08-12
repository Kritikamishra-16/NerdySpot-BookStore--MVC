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
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {

        private readonly AppDbContext _db;

        //passing the db parameter to the base class
        //constructor allows the derived class
        //CategoryRepository to reuse and extend the data
        //access functionality provided by the base
        //class Repository<Category>.
        public OrderHeaderRepository(AppDbContext db) : base(db) //calling the constructor of base class
        {
            _db = db;
        }
        
       

        public void Update(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);
        }

		public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
		{
            var orderFromDb=_db.OrderHeaders.FirstOrDefault(u=>u.Id==id);
            if(orderFromDb!=null)
            {
                orderFromDb.OrderStatus= orderStatus;
                if(!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus= paymentStatus;
                }
            }
		}

		public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
		{
			var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if(orderFromDb!=null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    orderFromDb.SessionId= sessionId;
                }
                if(!string.IsNullOrEmpty(paymentIntentId))
                {
                    orderFromDb.PaymentIntentId= paymentIntentId;
                    orderFromDb.PaymentDate = DateTime.Now;
                }
            }
		}
	}
}
