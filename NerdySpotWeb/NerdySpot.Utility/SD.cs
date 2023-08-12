using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerdySpot.Utility
{
    public static class SD
    {
        //Static details class it contains all the costants of our website
        public const string Role_Customer = "Customer";

        //COMPANY used will get 30 days to make payment after the order has been placed 
        //Aa company user can be registered by an ADMIN user
        public const string Role_Company = "Company";

        //ADMIN will be able to perform all the CRUD opetaions on product and other content management 
        public const string Role_Admin = "Admin";

        //EMPLOYEE will have access to may be modify the shipping of the product and other details
        public const string Role_Employee = "Employee";


		//ORDER STATUS
		public const string StatusPending = "Pending";
		public const string StatusAapproved = "Approved";
		public const string StatusInProcess = "Processing";
		public const string StatusShipped = "Shipped";
		public const string StatusCanceled = "Cancelled";
		public const string StatusRefunded = "Refunded";

		//PAYMENT STATUS
		public const string PaymentStatusPending = "Pending";
		public const string PaymentStatusApproved = "Approved";
		public const string PaymentStatusDelayedParyment = "ApprovedForDelayedPayment";
		public const string PaymentStatusRejected = "Rejected";

		//SESSION
		public const string SessionCart = "SessionShoppingCart";


	}
}
