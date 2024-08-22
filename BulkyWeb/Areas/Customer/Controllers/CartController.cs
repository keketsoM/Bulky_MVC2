using Bulky.DataAccess.Repository.Interface;
using Bulky.Model;
using Bulky.Model.ViewModel;
using Bulky.Utility;
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
		[BindProperty]
		public ShoppingCartVM shoppingCartVM { get; set; }
		public CartController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		public IActionResult Index()
		{
			var ClaimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			shoppingCartVM = new ShoppingCartVM()
			{
				ShoppingCartList = _unitOfWork.ShoppingCartRepo.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
				orderHeader = new OrderHeader() { }
			};
			foreach (var cartList in shoppingCartVM.ShoppingCartList)
			{
				cartList.CartTotalPrice = GetPriceBasedOnQuantity(cartList);
				shoppingCartVM.orderHeader.OrderTotal += (cartList.CartTotalPrice * cartList.Quantity);
			}

			return View(shoppingCartVM);
		}

		public IActionResult Summary()
		{
			var ClaimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			shoppingCartVM = new ShoppingCartVM()
			{
				ShoppingCartList = _unitOfWork.ShoppingCartRepo.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
				orderHeader = new OrderHeader() { }
			};

			shoppingCartVM.orderHeader.ApplicationUser = _unitOfWork.UserRepo.Get(u => u.Id == userId);
			shoppingCartVM.orderHeader.Name = shoppingCartVM.orderHeader.ApplicationUser.Name;
			shoppingCartVM.orderHeader.PhoneNumber = shoppingCartVM.orderHeader.ApplicationUser.PhoneNumber;
			shoppingCartVM.orderHeader.StreetAddress = shoppingCartVM.orderHeader.ApplicationUser.StreetAddress;
			shoppingCartVM.orderHeader.City = shoppingCartVM.orderHeader.ApplicationUser.City;
			shoppingCartVM.orderHeader.State = shoppingCartVM.orderHeader.ApplicationUser.State;
			shoppingCartVM.orderHeader.PostalCode = shoppingCartVM.orderHeader.ApplicationUser.PostalCode;

			foreach (var cartList in shoppingCartVM.ShoppingCartList)
			{
				cartList.CartTotalPrice = GetPriceBasedOnQuantity(cartList);
				shoppingCartVM.orderHeader.OrderTotal += (cartList.CartTotalPrice * cartList.Quantity);
			}

			return View(shoppingCartVM);
		}
		[HttpPost]
		[ActionName("Summary")]
		public IActionResult SummaryPost()
		{
			var ClaimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			shoppingCartVM.orderHeader.ApplicationUserId = userId;
			shoppingCartVM.orderHeader.OrderDate = DateTime.Now;

			ApplicationUser applicationUser = _unitOfWork.UserRepo.Get(u => u.Id == userId);



			foreach (var cartList in shoppingCartVM.ShoppingCartList)
			{
				cartList.CartTotalPrice = GetPriceBasedOnQuantity(cartList);
				shoppingCartVM.orderHeader.OrderTotal += (cartList.CartTotalPrice * cartList.Quantity);
			}

			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
				shoppingCartVM.orderHeader.OrderStatus = SD.StatusPending;
				shoppingCartVM.orderHeader.PaymentStatus = SD.PaymentStatusPending;
			}
			else
			{
				shoppingCartVM.orderHeader.OrderStatus = SD.StatusApproved;
				shoppingCartVM.orderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
			}
			_unitOfWork.OrderheaderRepo.Add(shoppingCartVM.orderHeader);
			_unitOfWork.save();

			foreach (var cart in shoppingCartVM.ShoppingCartList)
			{
				OrderDetail orderHeader = new OrderDetail()
				{

					Id = shoppingCartVM.orderHeader.Id,
					ProductId = cart.ProductId,
					Price = cart.Product.Price,
					Count = cart.Quantity
				};
				_unitOfWork.OrderdetailsRepo.Add(orderHeader);
				_unitOfWork.save();
			}
			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
				// it is a regular customer caputre the payment
				//stripe logic
			}
			return RedirectToAction(nameof(OrderConfimation), new { id = shoppingCartVM.orderHeader.Id });
		}
		public IActionResult OrderConfimation(int id)
		{
			return View(id);
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
