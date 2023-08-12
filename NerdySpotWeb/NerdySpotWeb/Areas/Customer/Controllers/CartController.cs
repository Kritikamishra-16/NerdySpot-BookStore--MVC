using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NerdySpot.DataAccess.Repository.IRepository;
using NerdySpot.Models;
using NerdySpot.Models.ViewModels;
using NerdySpot.Utility;
using Stripe.Checkout;
using System.Security.Claims;

namespace NerdySpotWeb.Areas.Customer.Controllers
{
    [Area("customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty] //when the details will be populated and we hit the submit button on summary page this
        //shopping cart VM will automatically be populated with those values and we not need to write this in summary post action method
		public ShoppingCartVM cartVM { get;set; }

		public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            //Retrieve a shopping cart for a user for that we need a userId
            
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId=claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            
            cartVM = new ()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"), //we also want to include product

                OrderHeader= new()

            };

            foreach(var cartItem in cartVM.ShoppingCartList)
            {
                cartItem.Price=GetPriceBasedOnQuantity(cartItem);
                cartVM.OrderHeader.OrderTotal += (cartItem.Price * cartItem.Count);
            }

            return View(cartVM);
        }

        public IActionResult Summary() {

            var claimsIdentity=(ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM cartVM = new()
            {
                ShoppingCartList= _unitOfWork.ShoppingCart
                .GetAll(u=>u.ApplicationUserId == userId, includeProperties: "Product"),

                OrderHeader= new()
            };

            cartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            cartVM.OrderHeader.Name = cartVM.OrderHeader.ApplicationUser.Name;
            cartVM.OrderHeader.PhoneNumber = cartVM.OrderHeader.ApplicationUser.PhoneNumber;
            cartVM.OrderHeader.State = cartVM.OrderHeader.ApplicationUser.State;
            cartVM.OrderHeader.StreetAddress = cartVM.OrderHeader.ApplicationUser.StreetAddress;
            cartVM.OrderHeader.City = cartVM.OrderHeader.ApplicationUser.City;
            cartVM.OrderHeader.PostalCode = cartVM.OrderHeader.ApplicationUser.PostalCode;


            foreach (var cartItem in cartVM.ShoppingCartList)
            {
                cartItem.Price = GetPriceBasedOnQuantity(cartItem);
                cartVM.OrderHeader.OrderTotal += (cartItem.Price * cartItem.Count);
            }

            return View(cartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
		public IActionResult SummaryPOST()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;


            cartVM.ShoppingCartList = _unitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");

			//all other etails are automatically populated bcz of property binding
			cartVM.OrderHeader.OrderDate= System.DateTime.Now;
            cartVM.OrderHeader.ApplicationUserId= userId;

            //DO NOT POPULATE NAVIGATON PROPERTY
			//cartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
			ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);


			foreach (var cartItem in cartVM.ShoppingCartList)
			{
				cartItem.Price = GetPriceBasedOnQuantity(cartItem);
				cartVM.OrderHeader.OrderTotal += (cartItem.Price * cartItem.Count);
			}

            //check if the user is company user so he will land up NET30! and directly placing the order instead of
            //landed up on the payment gateway

            if(applicationUser.CompanyId.GetValueOrDefault()==0)
            {
                //It is a regular customer account
                cartVM.OrderHeader.OrderStatus = SD.StatusPending;
                cartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            }
            else
            {
				//it is acompany account
				cartVM.OrderHeader.OrderStatus = SD.StatusAapproved;
				cartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedParyment;
			}

            _unitOfWork.OrderHeader.Add(cartVM.OrderHeader); //it will also add all the corresponding navigation property of the OrderHeader
            //bcz we have populated the navigaton property i.e.ApplicationUser  of OrderHeader if we dont want this we should never populate the navigation property
            //while inserting record in db
            _unitOfWork.Save();
            foreach(var cart in cartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    OrderHeaderId = cartVM.OrderHeader.Id,
                    ProductId = cart.ProductId,
                    Count = cart.Count,
                    Price = cart.Price,
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }

			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
                //It is a regular customer account we need to capture the payment
                //Stripe logic -> Payment processing platform
                //Stripe Doc code:-

                var domain = "https://localhost:7037/";

                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={cartVM.OrderHeader.Id}",
                    CancelUrl = domain + "customer/cart/Index",
                    LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
				};

                foreach(var item in cartVM.ShoppingCartList)
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
				Session session=service.Create(options);
                _unitOfWork.OrderHeader.UpdateStripePaymentID(cartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303); //redirecting to anew url provided by stripe
            }

            return RedirectToAction(nameof(OrderConfirmation), new { id = cartVM.OrderHeader.Id });
		}

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser");
            if(orderHeader.PaymentStatus !=SD.PaymentStatusDelayedParyment)
            {
                //this is an order by customer
                var service= new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if(session.PaymentStatus.ToLower()== "paid")
                {
                    //session is successful then update the paymentIntentId
					_unitOfWork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusAapproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }

                //after placing the order clear the session of cart count also
                //otherwise it will keep desplaying the previous cart count stored in the session
                HttpContext.Session.Clear();

            }


            //after successful payment empty the shopping cart
            List<ShoppingCart> shoppingcart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCart.RemoveRange(shoppingcart);
            _unitOfWork.Save();


            return View(id);
        }





		public IActionResult Plus(int cartId)
        {
            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            cartFromDb.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId, tracked: true);
            if (cartFromDb.Count <= 1)
            {
                //remove cartitem from shoppingcart

                //decrement the cart item count
                HttpContext.Session.SetInt32(SD.SessionCart,
                    _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);

                _unitOfWork.ShoppingCart.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId,tracked:true);

            //decrement the cart item count
            HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);

            _unitOfWork.ShoppingCart.Remove(cartFromDb);
            _unitOfWork.Save();

            

            return RedirectToAction(nameof(Index));
        }





        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
                
        }




    }
}
