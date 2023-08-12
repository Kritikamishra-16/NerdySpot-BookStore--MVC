using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NerdySpot.DataAccess.Repository.IRepository;
using NerdySpot.Models;
using NerdySpot.Utility;

namespace NerdySpotWeb.Areas.Admin.Controllers
{
    //we need to tellthe controller that this controller belongs to specificarea
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            //ToList() method on the Categories property of the _db
            //instance, which represents the "Categories" table in the
            //database. This method executes the query and retrieves
            //the categories as a list of Category objects.

            List<Category> objCategoryList = _unitOfWork.Category.GetAll().ToList(); //here we need to specify on which repo we are working on in unitfwork

            return View(objCategoryList);
        }

        //by default irt is a get action method [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost] //when the submit button will click on Create View
        public IActionResult Create(Category obj)
        {
            //Custom Validation and error messages
            /*if (obj.Name != null && obj.Name.ToLower() == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The DisplayOrder can not exactly match the Name");
            }
            if (obj.Name != null && obj.Name.ToLower() == "test")
            {
                ModelState.AddModelError("", "Test is an invalid value");
            }*/
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(obj);
                _unitOfWork.Save();

                TempData["success"] = "Category created successfully";

                return RedirectToAction("Index");
                //return RedirectToAction("Index","Category");
            }
            //if model state not valid return to view itself
            return View();

        }



        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            //if the Id is valid then retrieve that data from database so that
            //we cath patch those values in edit view form
            //Category? categoryFromDb1 = _db.Categories.FirstOrDefault(u => u.Id == id); //it can find by anything eg: u=>u.Name.contains
            //Category? categoryFromDb2 = _db.Categories.Where(u => u.Id == id).FirstOrDefault();
            //Category? categoryFromDb = _db.Categories.Find(id); //only find by primary key

            Category? categoryFromDb = _unitOfWork.Category.Get(u => u.Id == id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        [HttpPost] //when the submit button will click on Edit View
        public IActionResult Edit(Category obj)
        {
            //Always try to add a break point on post edit method
            //to check if all the obj fields are correctly
            //populated or not
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            //if model state not valid return to view itself
            return View();
        }



        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? categoryFromDb = _unitOfWork.Category.Get(u => u.Id == id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Category? obj = _unitOfWork.Category.Get(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _unitOfWork.Category.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
