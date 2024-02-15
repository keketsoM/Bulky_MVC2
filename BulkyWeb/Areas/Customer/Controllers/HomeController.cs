
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.Interface;
using Bulky.Model;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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
            var products = _unitOfWork.ProductRepo.GetAll(includeProperties:"Category").ToList();

            return View(products);
        }

        public IActionResult Detail(int id)
        {
            var product = _unitOfWork.ProductRepo.Get(u=>u.Id==id,includeProperties:"Category");
            return View(product);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}