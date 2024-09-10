using Bulky.DataAccess.Repository.Interface;
using Bulky.Model;
using Bulky.Model.ViewModel;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        [BindProperty]
        public OrderVM ordervm { get; set; }
        public OrderController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index(string status)
        {
            IEnumerable<OrderHeader> orderHeader;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeader = _unitOfWork.OrderheaderRepo.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                ClaimsIdentity claimIdentity = (ClaimsIdentity)User.Identity;
                var claimUserId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                orderHeader = _unitOfWork.OrderheaderRepo.GetAll(u => u.ApplicationUserId == claimUserId, includeProperties: "ApplicationUser");
            }
            if (!string.IsNullOrEmpty(status))
            {
                switch (status)
                {
                    case "paymentPending":
                        orderHeader = orderHeader.Where(o => o.PaymentStatus == SD.StatusPending);
                        break;
                    case "inprocess":
                        orderHeader = orderHeader.Where(o => o.OrderStatus == SD.StatusInProcess);
                        break;
                    case "completed":
                        orderHeader = orderHeader.Where(o => o.PaymentStatus == SD.StatusShipped);
                        break;
                    case "approved":
                        orderHeader = orderHeader.Where(o => o.PaymentStatus == SD.StatusApproved);
                        break;
                    default:
                        break;
                }
            }


            return View(orderHeader);
        }
        public IActionResult Detail(int id)
        {
            var ordervm = new OrderVM()
            {
                orderHeader = _unitOfWork.OrderheaderRepo.Get(u => u.Id == id, includeProperties: "ApplicationUser"),
                orderDetails = _unitOfWork.OrderdetailsRepo.GetAll(u => u.OrderHeaderId == id, includeProperties: "Product"),
            };

            return View(ordervm);

        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _unitOfWork.OrderheaderRepo.Get(u => u.Id == ordervm.orderHeader.Id);

            orderHeaderFromDb.Name = ordervm.orderHeader.Name;
            orderHeaderFromDb.PhoneNumber = ordervm.orderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = ordervm.orderHeader.StreetAddress;
            orderHeaderFromDb.City = ordervm.orderHeader.City;
            orderHeaderFromDb.State = ordervm.orderHeader.State;
            orderHeaderFromDb.PostalCode = ordervm.orderHeader.PostalCode;
            if (string.IsNullOrEmpty(orderHeaderFromDb.Carrier))
            {
                orderHeaderFromDb.Carrier = ordervm.orderHeader.Carrier;
            }
            if (string.IsNullOrEmpty(orderHeaderFromDb.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = ordervm.orderHeader.TrackingNumber;
            }
            _unitOfWork.OrderheaderRepo.Update(orderHeaderFromDb);
            _unitOfWork.save();
            TempData["Success"] = "Order Updated Successfully";
            return RedirectToAction("Detail", new { id = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderheaderRepo.UpdateStatus(ordervm.orderHeader.Id, orderstatus: SD.StatusInProcess);
            _unitOfWork.save();
            TempData["Success"] = "Order Processed Successfully";
            return RedirectToAction("Detail", new { id = ordervm.orderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShippedOrder()
        {
            var orderHeaderFromDb = _unitOfWork.OrderheaderRepo.Get(u => u.Id == ordervm.orderHeader.Id, includeProperties: "ApplicationUser");

            orderHeaderFromDb.TrackingNumber = ordervm.orderHeader.TrackingNumber;
            orderHeaderFromDb.Carrier = ordervm.orderHeader.Carrier;
            orderHeaderFromDb.ShippingDate = DateTime.Now;
            orderHeaderFromDb.OrderStatus = SD.StatusShipped;

            if (orderHeaderFromDb.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeaderFromDb.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }



            _unitOfWork.OrderheaderRepo.Update(orderHeaderFromDb);
            _unitOfWork.save();
            TempData["Success"] = "Order Shipped Successfully";
            return RedirectToAction("Detail", new { id = ordervm.orderHeader.Id });
        }
    }
}
