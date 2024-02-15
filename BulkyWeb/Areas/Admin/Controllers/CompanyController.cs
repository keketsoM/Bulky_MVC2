
using Bulky.DataAccess.Repository.Interface;
using Bulky.Model;
using Bulky.Model.ViewModel;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Drawing;

namespace BulkyWeb.Areas.Admin.Controllers
{

    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }
        public IActionResult Index()
        {
            List<Company> companyObj = _unitOfWork.CompanyRepo.GetAll().ToList();
            return View(companyObj);
        }
        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            Company companyObj = new Company();
            if (id == null || id == 0)
            {
                return View(companyObj);
            }
            else
            {
                companyObj = _unitOfWork.CompanyRepo.Get(p => p.Id == id);
                return View(companyObj);
            }

            // ViewBag.CategoryList = categoryList;   
            //  return View(product);
        }
        [HttpPost]
        public IActionResult Upsert(Company companyObj)
        {
            if (ModelState.IsValid)
            {
                if (companyObj.Id == 0)
                {
                    _unitOfWork.CompanyRepo.Add(companyObj);
                }
                else
                {
                    _unitOfWork.CompanyRepo.Update(companyObj);
                }
                _unitOfWork.save();
                TempData["Success"] = "Company created successfully";
                return RedirectToAction("Index");
            }
            else
            {
                return View("Upsert", companyObj);
            }

        }

        public ActionResult Delete(int id)
        {

            if (id == null || id == 0)
            {
                ModelState.AddModelError("", "Id is null or zero");

            }
            else
            {
                var company = _unitOfWork.CompanyRepo.Get(p => p.Id == id);
                if (company == null)
                {
                    NotFound();
                }
                else
                {
                    _unitOfWork.CompanyRepo.Remove(company);
                    _unitOfWork.save();
                    TempData["Success"] = "Company deleted successfully";
                    return RedirectToAction("Index");
                }

            }
            return View("Delete");

        }
    }
}

