using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NerdySpot.DataAccess.Repository.IRepository;
using NerdySpot.Models;
using NerdySpot.Utility;
using System.Diagnostics;
using System.Security.Claims;

namespace NerdySpotWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            //when the user logs in display their cart count

            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category");
            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCart cart = new()
            {
                Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category"),
                Count = 1,
                ProductId = productId
            };
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart cartItem)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId= claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            cartItem.ApplicationUserId = userId;

            //when we retrieve something form EF core it is constantly tracking that
            //If we dont want constant tracking we have to change default behaviour in repository
            // T Get(bool tracked=false)
            ShoppingCart cartItemFromDb = _unitOfWork.ShoppingCart.Get(u =>u.ApplicationUserId==cartItem.ApplicationUserId && u.ProductId == cartItem.ProductId); //if the same user has already the same product in the cart then only update its count instead f adding a new duplicate product
            
            if(cartItemFromDb == null)
            {
                //add cart record
                _unitOfWork.ShoppingCart.Add(cartItem);
                _unitOfWork.Save();

                HttpContext.Session.SetInt32(SD.SessionCart,
                    _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count());
            }
            else
            {
                //cart item already exist
                cartItemFromDb.Count += cartItem.Count;
                _unitOfWork.ShoppingCart.Update(cartItemFromDb);
                _unitOfWork.Save();


            }
            TempData["success"] = "Cart updated Successfully";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}