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

    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Company> CompanysObj = _unitOfWork.Company.GetAll().ToList();
            
            return View(CompanysObj);
        }

        public IActionResult Upsert(int? id) //Update+Insert= Upsert
        {
            if(id==null || id==0)
            {
                //We are creating a Company
                return View(new Company());
            }
            else
            {
                //We are updating a Company
                Company companyObj= _unitOfWork.Company.Get(u=>u.Id==id);
                return View(companyObj);
            }

        }

        [HttpPost]
        public IActionResult Upsert(Company obj)
        {
            
            if (ModelState.IsValid)
            {  
                
                if(obj.Id==0)
                {
                    //This means it is an add
                    _unitOfWork.Company.Add(obj);

                }
                else
                {
                    //update
                    _unitOfWork.Company.Update(obj);
                }

                _unitOfWork.Save();
                TempData["success"] = "Company created successfully!";
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
                Company CompanyObj = _unitOfWork.Company.Get(u => u.Id == id);
                if (CompanyObj == null)
                {
                    return NotFound();
                }

                return View(CompanyObj);
            }

            [HttpPost, ActionName("Delete")]
            public IActionResult DeletePOST(int? id)
            {
                Company CompanyObj = _unitOfWork.Company.Get(u => u.Id == id);
                if (CompanyObj == null)
                {
                    return NotFound();
                }
                _unitOfWork.Company.Remove(CompanyObj);
                _unitOfWork.Save();

                TempData["success"] = "Company deleted successfully!";
                return RedirectToAction("Index");

            }
    }
}
