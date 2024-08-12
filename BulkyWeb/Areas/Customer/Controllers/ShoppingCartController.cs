
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.Interface;
using Bulky.Model;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ShoppingCartController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(int id)
        {
            ShoppingCart shoppingCart = new ShoppingCart()
            {
                product = _unitOfWork.ProductRepo.Get(p => p.Id == id, includeProperties: "Category"),
                Quantity = 1,
                ProductId = id,
            };

            return View(shoppingCart);
        }

        


    }
}
