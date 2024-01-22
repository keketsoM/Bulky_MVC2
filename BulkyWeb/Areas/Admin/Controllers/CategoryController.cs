using Bulky.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.Interface;

namespace BulkyWeb.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var listCategory = _unitOfWork.CategoryRepo.GetAll().ToList();
            return View(listCategory);
        }
        [HttpGet]
        public IActionResult Create()
        {

            return View();
        }
        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (category.Name == null || category.DisplayOrder == 0)
            {
                ModelState.AddModelError("name", "Name is null.");
                ModelState.AddModelError("DisplayOrder", "Number is zero.");
                return View("Create");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    _unitOfWork.CategoryRepo.Add(category);
                    _unitOfWork.save();
                    TempData["Success"] = "Category created successfully";
                }
                else
                {
                    return View("Create");
                }


            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                ModelState.AddModelError("", "Id is null or zero");
                return View("Edit");
            }
            else
            {
                
                var category = _unitOfWork.CategoryRepo.Get(p => p.CategoryId == id);
                return View(category);
            }

        }
        [HttpPost]
        public ActionResult Edit(Category category)
        {

            if (ModelState.IsValid)
            {
                _unitOfWork.CategoryRepo.Update(category);
                _unitOfWork.save();
                TempData["Success"] = "Category edited successfully";
                return RedirectToAction("Index");
            }



            return View("Edit");
        }

        public ActionResult Detail(int id)
        {
            var category = _unitOfWork.CategoryRepo.Get(p => p.CategoryId == id);
            return View(category);
        }
        [HttpGet]
        public ActionResult Delete(int id)
        {

            var category = _unitOfWork.CategoryRepo.Get(p => p.CategoryId == id);
            return View(category);
        }
        [HttpPost]
        public ActionResult Delete(int? id)
        {

            if (id == null || id == 0)
            {
                ModelState.AddModelError("", "Id is null or zero");

            }
            else
            {
                var category = _unitOfWork.CategoryRepo.Get(p => p.CategoryId == id);
                if (category == null)
                {
                    NotFound();
                }
                else
                {
                    _unitOfWork.CategoryRepo.Remove(category);
                    _unitOfWork.save();
                    TempData["Success"] = "Category deleted successfully";
                    return RedirectToAction("Index");
                }

            }
            return View("Delete");

        }
    }
}
