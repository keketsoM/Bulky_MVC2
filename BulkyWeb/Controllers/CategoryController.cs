using Bulky.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Bulky.DataAccess.Data;

namespace BulkyWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var listCategory = _context.categories.ToList();
            return View(listCategory);
        }
        [HttpGet]
        public IActionResult Create()
        {

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Category category)
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
                    _context.categories.Add(category);
                    await _context.SaveChangesAsync();
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
                var category = _context.categories.FirstOrDefault(p => p.CategoryId == id);
                return View(category);
            }

        }
        [HttpPost]
        public async Task<ActionResult> Edit(Category category)
        {

            if (ModelState.IsValid)
            {
                _context.categories.Update(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Category edited successfully";
                return RedirectToAction("Index");
            }



            return View("Edit");
        }

        public ActionResult Detail(int id)
        {
            var category = _context.categories.FirstOrDefault(p => p.CategoryId == id);
            return View(category);
        }
        [HttpGet]
        public ActionResult Delete(int id)
        {

            var category = _context.categories.FirstOrDefault(p => p.CategoryId == id);
            return View(category);
        }
        [HttpPost]
        public async Task<ActionResult> Delete(int? id)
        {

            if (id == null || id == 0)
            {
                ModelState.AddModelError("", "Id is null or zero");

            }
            else
            {
                var category = _context.categories.FirstOrDefault(p => p.CategoryId == id);
                if (category == null)
                {
                    NotFound();
                }
                else
                {
                    _context.Remove(category);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Category deleted successfully";
                    return RedirectToAction("Index");
                }

            }
            return View("Delete");

        }
    }
}
