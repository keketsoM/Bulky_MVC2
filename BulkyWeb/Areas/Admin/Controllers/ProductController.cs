using Bulky.DataAccess.Repository.Interface;
using Bulky.Model;
using Bulky.Model.ViewModel;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;



namespace BulkyWeb.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
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
            var listProduct = _unitOfWork.ProductRepo.GetAll(includeProperties: "Category").ToList();
            return View(listProduct);
        }
        [HttpGet]
        public IActionResult Upsert(int? id)
        {

            IEnumerable<SelectListItem> categoryList = _unitOfWork.CategoryRepo.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.CategoryId.ToString()
            });
            ProductVM productvm = new()
            {
                categoryList = categoryList,
                product = new Product()
            };

            if (id == null || id == 0)
            {
                return View(productvm);
            }
            else
            {
                productvm.product = _unitOfWork.ProductRepo.Get(p => p.Id == id, includeProperties: "ProductImages");
                return View(productvm);
            }

            // ViewBag.CategoryList = categoryList;   
            //  return View(product);
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productvm, List<IFormFile>? files)
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

                    if (productvm.product.Id == 0)
                    {
                        _unitOfWork.ProductRepo.Add(productvm.product);
                    }
                    else
                    {
                        _unitOfWork.ProductRepo.Update(productvm.product);
                    }

                    _unitOfWork.save();
                    if (files != null)
                    {
                        foreach (IFormFile file in files)
                        {
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            string productPath = @"images\products\product-" + productvm.product.Id;
                            string finalPath = Path.Combine(wwwRootPath, productPath);

                            if (!Directory.Exists(finalPath))
                            {
                                Directory.CreateDirectory(finalPath);
                            }

                            using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                            {
                                file.CopyTo(fileStream);
                            }
                            ProductImage productImage = new ProductImage
                            {
                                ImageUrl = @"\" + productPath + @"\" + fileName,
                                ProductId = productvm.product.Id,
                            };
                            if (productvm.product.ProductImages == null)
                            {
                                productvm.product.ProductImages = new List<ProductImage>();
                            }
                            productvm.product.ProductImages.Add(productImage);
                            _unitOfWork.ProductImageRepo.Add(productImage);
                            _unitOfWork.save();

                        }


                        //single image upload
                        //if (!string.IsNullOrEmpty(productvm.product.ImageUrl))
                        //{
                        //    var oldImagePath = Path.Combine(wwwRootPath, productvm.product.ImageUrl.TrimStart('\\'));
                        //    if (System.IO.File.Exists(oldImagePath))
                        //    {
                        //        System.IO.File.Delete(oldImagePath);
                        //    }
                        //}
                        //using (var fileStream = new FileStream(Path.Combine(productPath, productName), FileMode.Create))
                        //{
                        //    file.CopyTo(fileStream);
                        //}
                        //productvm.product.ImageUrl = @"\images\product\" + productName;
                    }

                    TempData["Success"] = "Product created/updated successfully";
                }
                else
                {
                    productvm.categoryList = _unitOfWork.CategoryRepo.GetAll().Select(u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.CategoryId.ToString()
                    });

                    return View("Create", productvm);
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


                    string productPath = @"images\products\product-" + id;
                    string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);

                    if (!Directory.Exists(finalPath))
                    {
                        var filepaths = Directory.GetFiles(finalPath);
                        foreach (var filepath in filepaths)
                        {
                            System.IO.File.Delete(filepath);
                        }
                        Directory.Delete(finalPath);
                    }
                    _unitOfWork.ProductRepo.Remove(product);
                    _unitOfWork.save();
                    TempData["Success"] = "Category deleted successfully";
                    return RedirectToAction("Index");
                }

            }
            return View("Delete");

        }
        [HttpGet]
        public ActionResult DeleteImage(int imageId)
        {

            var productImage = _unitOfWork.ProductImageRepo.Get(p => p.Id == imageId);

            if (productImage != null)
            {
                if (!string.IsNullOrEmpty(productImage.ImageUrl))
                {

                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productImage.ImageUrl.Trim('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }

                }
            }
            _unitOfWork.ProductImageRepo.Remove(productImage);
            _unitOfWork.save();
            TempData["Success"] = "Image Deleted Successfully";
            return RedirectToAction(nameof(Upsert), new { id = productImage.ProductId });
        }

    }
}

