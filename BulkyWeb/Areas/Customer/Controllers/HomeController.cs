
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.Interface;
using Bulky.Model;
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
            var products = _unitOfWork.ProductRepo.GetAll(includeProperties: "Category").ToList();

            return View(products);
        }

        public IActionResult Detail(int id)
        {
            ShoppingCart shoppingCart = new ShoppingCart()
            {
                product = _unitOfWork.ProductRepo.Get(p => p.Id == id, includeProperties: "Category"),
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
            _unitOfWork.ShoppingCartRepo.Add(shoppingCart);
            _unitOfWork.save();

            return View();


        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
