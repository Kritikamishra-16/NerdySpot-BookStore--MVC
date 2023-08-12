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
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);

        void UpdateStatus(int id, string orderStatus, string? paymentStatus=null);

        void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);
    }
}
