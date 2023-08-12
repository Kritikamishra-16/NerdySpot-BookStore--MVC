using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using NerdySpot.DataAccess.Repository.IRepository;
using NerdySpot.Models;
using NerdySpot.Models.ViewModels;
using NerdySpot.Utility;
using System.Data;

namespace NerdySpotWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        //TO access the root path of our application
        private readonly  IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> ProductsObj = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            
            return View(ProductsObj);
        }

        public IActionResult Upsert(int? id) //Update+Insert= Upsert
        {
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
            {
                //Projection in EF Core
                Text = u.Name,
                Value = u.Id.ToString()
            });

            //ViewBag.CategoryList = CategoryList;

            //ViewData["CategoryList"] = CategoryList;

            //VIEW MODEL (STRONGLY TYPED VIEW)
            ProductViewModel productVM = new()
            {
                Product = new Product(),
                CategoryList = CategoryList
            };

            if(id==null || id==0)
            {
                //We are creating a product
                return View(productVM);
            }
            else
            {
                //We are updating a product
                productVM.Product= _unitOfWork.Product.Get(u=>u.Id==id);
                return View(productVM);
            }

        }

        [HttpPost]
        public IActionResult Upsert(ProductViewModel obj, IFormFile? file)
        {

            
            if (ModelState.IsValid)
            {  
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if(file!=null)
                {
                    //this will give the new random file name with uploaded file extention
                    string fileName=Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        
                    //file path where we want to save thefile
                    string productPath=Path.Combine(wwwRootPath, @"images\product");
                    
                    /*Update product logic*/
                    if(!string.IsNullOrEmpty(obj.Product.ImageUrl))
                    {
                        //if old image is there and there is new image file uploaded
                        //so we need to delete the old image and create a new one
                        var oldImagePath = 
                            Path.Combine(wwwRootPath,obj.Product.ImageUrl.TrimStart('\\'));

                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath,fileName), FileMode.Create)) //Complete url (path+filename) , mode->create(as we are creating here)
                    {
                        file.CopyTo(fileStream);
                    }

                    obj.Product.ImageUrl = @"\images\product\"+ fileName;
                }

                if(obj.Product.Id==0)
                {
                    //This means it is an add
                    _unitOfWork.Product.Add(obj.Product);

                }
                else
                {
                    //update
                    _unitOfWork.Product.Update(obj.Product);
                }

                _unitOfWork.Save();
                TempData["success"] = "Product created successfully!";
                return RedirectToAction("Index");
            }
            return View();
        }


            public IActionResult Delete(int? id)
            {
                if (id == null || id == 0)
                {
                    return NotFound();
                }
                Product productObj = _unitOfWork.Product.Get(u => u.Id == id);
                if (productObj == null)
                {
                    return NotFound();
                }

                return View(productObj);
            }

            [HttpPost, ActionName("Delete")]
            public IActionResult DeletePOST(int? id)
            {
                Product productObj = _unitOfWork.Product.Get(u => u.Id == id);
                if (productObj == null)
                {
                    return NotFound();
                }
                _unitOfWork.Product.Remove(productObj);
                _unitOfWork.Save();

                TempData["success"] = "Product deleted successfully!";
                return RedirectToAction("Index");

            }
    }
}
