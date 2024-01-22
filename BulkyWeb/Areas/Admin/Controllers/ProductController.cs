using Bulky.DataAccess.Repository.Interface;
using Bulky.Model;
using Bulky.Model.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Drawing;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;   
        }
        public IActionResult Index()
        {
            var listProduct = _unitOfWork.ProductRepo.GetAll(includeProperties:"Category").ToList();
            return View(listProduct);
        }
        [HttpGet]
        public IActionResult Upsert(int? id)
        {
           
            IEnumerable<SelectListItem> categoryList=_unitOfWork.CategoryRepo.GetAll().Select(u=>new SelectListItem
           {
               Text = u.Name,
               Value=u.CategoryId.ToString()
           });
            ProductVM productvm = new() {
                categoryList = categoryList,
                product = new() { }
            };
            
            if (id == null || id == 0)
            {
                return View(productvm); 
            }
            else
            {
                 productvm.product = _unitOfWork.ProductRepo.Get(p => p.Id == id);
                return View(productvm);
            }

            // ViewBag.CategoryList = categoryList;   
          //  return View(product);
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productvm, IFormFile? file)
        {
            if (productvm.product.Title == null || productvm.product.Price == 0)
            {
                ModelState.AddModelError("name", "Name is null.");
                ModelState.AddModelError("DisplayOrder", "Number is zero.");
                return View("Create");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    if (file != null)
                    {
                       string productName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                       string productPath = Path.Combine(wwwRootPath,@"images\product");
                        if (!string.IsNullOrEmpty(productvm.product.ImageUrl))
                        {
                            var oldImagePath = Path.Combine(wwwRootPath,productvm.product.ImageUrl.TrimStart('\\'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }
                        using (var fileStream = new FileStream(Path.Combine(productPath, productName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }
                        productvm.product.ImageUrl = @"\images\product\" + productName;
                    }
                    if (productvm.product.Id == 0)
                    {
                        _unitOfWork.ProductRepo.Add(productvm.product);
                    }
                    else
                    {
                        _unitOfWork.ProductRepo.Update(productvm.product);
                    }
                  
                    _unitOfWork.save();
                    TempData["Success"] = "Category created successfully";
                }
                else
                {
                    productvm.categoryList = _unitOfWork.CategoryRepo.GetAll().Select(u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.CategoryId.ToString()
                    });
                  
                    return View("Create",productvm);
                }


            }
            return RedirectToAction("Index");
        }
      

        public ActionResult Detail(int id)
        {
            var product = _unitOfWork.ProductRepo.Get(p => p.Id == id);
            return View(product);
        }
        [HttpGet]
        public ActionResult Delete(int id)
        {

            var product = _unitOfWork.ProductRepo.Get(p => p.Id == id);
            return View(product);
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
                var product = _unitOfWork.ProductRepo.Get(p => p.Id == id);
                if (product == null)
                {
                    NotFound();
                }
                else
                {
                    _unitOfWork.ProductRepo.Remove(product);
                    _unitOfWork.save();
                    TempData["Success"] = "Category deleted successfully";
                    return RedirectToAction("Index");
                }

            }
            return View("Delete");

        }
    }
}

