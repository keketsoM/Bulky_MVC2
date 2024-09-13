
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.Interface;
using Bulky.Model;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
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
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if(userId != null)
            {
                HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCartRepo.GetAll(u => u.ApplicationUserId == userId).Count());
            }
            var products = _unitOfWork.ProductRepo.GetAll(includeProperties: "Category").ToList();

            return View(products);
        }

        public IActionResult Detail(int id)
        {
            ShoppingCart shoppingCart = new ShoppingCart()
            {
                Product = _unitOfWork.ProductRepo.Get(p => p.Id == id, includeProperties: "Category"),
                Quantity = 1,
                ProductId = id,
            };
            return View(shoppingCart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;
            ShoppingCart cartDb = _unitOfWork.ShoppingCartRepo.Get(
                u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId
            );
            if (cartDb != null)
            {
                cartDb.Quantity += shoppingCart.Quantity;
                _unitOfWork.ShoppingCartRepo.Update(cartDb);
                _unitOfWork.save();
            }
            else
            {
                _unitOfWork.ShoppingCartRepo.Add(shoppingCart);
                _unitOfWork.save();
                HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCartRepo.GetAll(u => u.ApplicationUserId == userId).Count());
            }



            TempData["Success"] = "Item add to Cart successfully";
            return RedirectToAction(nameof(Index));


        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
