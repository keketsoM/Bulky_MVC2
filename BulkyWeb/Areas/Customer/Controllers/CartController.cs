using Bulky.DataAccess.Repository.Interface;
using Bulky.Model;
using Bulky.Model.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var ClaimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM shoppingCartVM = new ShoppingCartVM()
            {
                ShoppingCartList = _unitOfWork.ShoppingCartRepo.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product")

            };
            foreach (var cartList in shoppingCartVM.ShoppingCartList)
            {
                cartList.CartTotalPrice = GetPriceBasedOnQuantity(cartList);
                shoppingCartVM.OrderTotal += (cartList.CartTotalPrice * cartList.Quantity);
            }

            return View(shoppingCartVM);
        }
        public IActionResult Plus(int cartId)
        {
            var shoppingCart = _unitOfWork.ShoppingCartRepo.Get(u => u.Id == cartId);
            shoppingCart.Quantity += 1;
            _unitOfWork.ShoppingCartRepo.Update(shoppingCart);
            _unitOfWork.save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int cartId)
        {
            var shoppingCart = _unitOfWork.ShoppingCartRepo.Get(u => u.Id == cartId);
            if (shoppingCart.Quantity > 0)
            {
                shoppingCart.Quantity -= 1;
                _unitOfWork.ShoppingCartRepo.Update(shoppingCart);
            }
            else
            {
                _unitOfWork.ShoppingCartRepo.Remove(shoppingCart);
            }

            _unitOfWork.save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int cartId)
        {
            var shoppingCart = _unitOfWork.ShoppingCartRepo.Get(u => u.Id == cartId);
            _unitOfWork.ShoppingCartRepo.Remove(shoppingCart);
            _unitOfWork.save();
            return RedirectToAction(nameof(Index));
        }
        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Quantity <= 0)
            {
                return shoppingCart.Product.Price;
            }
            else if (shoppingCart.Quantity <= 100)
            {
                return shoppingCart.Product.Price50;
            }
            else
            {
                return shoppingCart.Product.Price100;
            }

        }
    }
}
