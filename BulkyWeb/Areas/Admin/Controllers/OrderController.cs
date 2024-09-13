using Bulky.DataAccess.Repository.Interface;
using Bulky.Model;
using Bulky.Model.ViewModel;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
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

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {

            var orderHeader = _unitOfWork.OrderheaderRepo.Get(u => u.Id == ordervm.orderHeader.Id);

            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {

                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);
                _unitOfWork.OrderheaderRepo.UpdateStatus(ordervm.orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderheaderRepo.UpdateStatus(ordervm.orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }

            _unitOfWork.save();
            TempData["Success"] = "Order Cancelled Successfully";
            return RedirectToAction(nameof(Detail), new { id = ordervm.orderHeader.Id });
        }
        [HttpPost]

        public IActionResult DetailPayNow()
        {


            ordervm.orderHeader = _unitOfWork.OrderheaderRepo.Get(
                u => u.Id == ordervm.orderHeader.Id, includeProperties: "ApplicationUser");
            ordervm.orderDetails = _unitOfWork.OrderdetailsRepo.GetAll
                (u => u.OrderHeaderId == ordervm.orderHeader.Id, includeProperties: "Product");

            var domainUrl = "https://localhost:44398/";
            var options = new Stripe.Checkout.SessionCreateOptions
            {

                SuccessUrl = $"{domainUrl}admin/order/PaymentConfimation?orderHeaderId={ordervm.orderHeader.Id}",
                CancelUrl = $"{domainUrl}admin/order/detail?id={ordervm.orderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in ordervm.orderDetails)
            {
                var sessionLineItem = new Stripe.Checkout.SessionLineItemOptions
                {

                    PriceData = new SessionLineItemPriceDataOptions
                    {

                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title,

                        },


                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }
            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrderheaderRepo.UpdateStripePaymentID(ordervm.orderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        [HttpPost]
        public IActionResult PaymentConfimation(int id)
        {
            // this order by company
            var orderHeader = _unitOfWork.OrderheaderRepo.Get(u => u.Id == id, includeProperties: "ApplicationUser");
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderheaderRepo.UpdateStripePaymentID(orderHeader.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderheaderRepo.UpdateStatus(orderHeader.Id, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.save();
                }
            }

            return View(orderHeader.Id);
        }
    }
}
