using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NerdySpot.DataAccess.Repository.IRepository;
using NerdySpot.Models;
using NerdySpot.Models.ViewModels;
using NerdySpot.Utility;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace NerdySpotWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderVM orderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
		{
			return View();
		}

        public IActionResult Details(int orderId)
        {
             orderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail= _unitOfWork.OrderDetail.GetAll(u=>u.OrderHeaderId== orderId, includeProperties:"Product")
            };

            return View(orderVM);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin +","+SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);
            //map all the fields we want to update
            orderHeaderFromDb.Name= orderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber= orderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress= orderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City= orderVM.OrderHeader.City;
            orderHeaderFromDb.State= orderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode= orderVM.OrderHeader.PostalCode;
            if(!string.IsNullOrEmpty(orderVM.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = orderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(orderVM.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            }

            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["success"] = "Order Details Updated Successfully.";

            return RedirectToAction(nameof(Details), new {orderId= orderHeaderFromDb.Id});
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["success"] = "Order Status Updated Successfully.";

            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });

        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeaderFromDb=_unitOfWork.OrderHeader.Get(u=>u.Id==orderVM.OrderHeader.Id);
            orderHeaderFromDb.OrderStatus = SD.StatusShipped;
            orderHeaderFromDb.TrackingNumber=orderVM.OrderHeader.TrackingNumber;
            orderHeaderFromDb.Carrier = orderVM.OrderHeader.Carrier;
            orderHeaderFromDb.ShippingDate = DateTime.Now;
            if (orderHeaderFromDb.PaymentStatus == SD.PaymentStatusDelayedParyment)
            {

               // DateTime futureDateTime = DateTime.Now.AddDays(30);
               // string futureDate = futureDateTime.ToString("yyyy-MM-dd");
                orderHeaderFromDb.PaymentDueDate = DateTime.Now.AddDays(30);
            }


            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Order Shipped Successfully.";

            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });

        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeader= _unitOfWork.OrderHeader.Get(u=>u.Id== orderVM.OrderHeader.Id);

            if(orderHeader.PaymentStatus==SD.PaymentStatusApproved)
            {
                //ORDER CANCEL + we need to make a refund to the same intent id using STRIPE
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service= new RefundService();
                Refund refund=service.Create(options);

                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id,SD.StatusCanceled, SD.StatusRefunded );

            }
            else
            {
                //ORDER CANCEL + NO REFUND as no payment has been done
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id,SD.StatusCanceled, SD.StatusCanceled );

            }
            _unitOfWork.Save();
            TempData["success"] = "Order Cancelled Successfully.";

            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.Id });



        }


        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_PAY_NOW()
        {

            orderVM.OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            orderVM.OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderVM.OrderHeader.Id, includeProperties: "Product");

            var domain = "https://localhost:7037/";

            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={orderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={orderVM.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in orderVM.OrderDetail)
            {
                var SessionLineItemOptions = new SessionLineItemOptions
                {
                    //PriceData is a data used to create a new price object
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), //$20.50=> 2050
                        Currency = "inr",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(SessionLineItemOptions);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrderHeader.UpdateStripePaymentID(orderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

        }


        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedParyment)
            {
                //this is an order by company
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    //session is successful then update the paymentIntentId
                    _unitOfWork.OrderHeader.UpdateStripePaymentID(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }

           
            return View(orderHeaderId);
        }



        #region API CALLS

        [HttpGet]
        public IActionResult GetAll(string status)
		{
            IEnumerable<OrderHeader> objOrderHeaders;


            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objOrderHeaders = _unitOfWork.OrderHeader
                    .GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }
			switch (status)
			{
                case "pending":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedParyment);
                    break;
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(u=>u.OrderStatus== SD.StatusInProcess);
                    break;
                case "completed":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusAapproved);
                    break;
                default:
                    break;
            }
			

			return Json(new {data=objOrderHeaders}); //returning json new object
		}

        #endregion
    }
}
