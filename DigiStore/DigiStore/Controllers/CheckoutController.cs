using DigiStore.Data;
using DigiStore.Models;
using DigiStore.Services.CartServices;
using DigiStore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DigiStore.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly AppDbContext _context;

        public CheckoutController(ICartService cartService, AppDbContext context)
        {
            _cartService = cartService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Logout", "Account");
            }

            var userId = int.Parse(userIdString);

            var cart = await _cartService.GetOrCreateCartAsync();
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var cartVm = new CartViewModel
            {
                Items = cart.Items.Select(i => new CartItemViewModel
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    Title = i.Product.Title,
                    ImageName = i.Product.MainImageName,
                    Price = (long)i.Price, 
                    Quantity = i.Quantity
                }).ToList()
            };

            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var vm = new CheckoutViewModel
            {
                Cart = cartVm,
                UserAddresses = addresses
            };

            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> AddAddress(Address address)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (ModelState.IsValid)
            {
                address.UserId = userId;

                address.FullAddress = $"{address.Province}، {address.City}، {address.FullAddress}";

                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult PaymentResult(string msg,decimal finalAmount)
        {
            var vm = new PaymentViewMode();
            vm.msg = msg;
            vm.finalAmount = finalAmount;
            return View(vm);
        }



        [HttpPost]
        public async Task<IActionResult> PaymentPage(int SelectedAddressId)
        {
            var vm = new PaymentViewMode();
            vm.SelectedAddressId = SelectedAddressId;
           return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Payment(int? SelectedAddressId)
        {
            if (SelectedAddressId == null)
            {
                TempData["Error"] = "لطفا یک آدرس انتخاب کنید.";
                return RedirectToAction("Index");
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Logout", "Account");
            }

            var userId = int.Parse(userIdString);

            var cart = await _cartService.GetOrCreateCartAsync();
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            decimal totalAmount = cart.Items.Sum(i => (decimal)i.Price * i.Quantity);
            decimal shippingCost = 0;
            decimal finalAmount = totalAmount + shippingCost;

            var walletService = HttpContext.RequestServices.GetService<IWalletService>();
            if (walletService == null)
            {
                TempData["Error"] = "سیستم کیف پول در دسترس نیست.";
                return RedirectToAction("Index");
            }

            var walletBalance = await walletService.GetBalanceAsync(userId);

            if (walletBalance < finalAmount)
            {
                var shortage = finalAmount - walletBalance;

                TempData["Error"] = "موجودی کیف پول شما کافی نیست!";
                TempData["Shortage"] = shortage.ToString();
                TempData["WalletBalance"] = walletBalance.ToString(); 
                TempData["NeededAmount"] = finalAmount.ToString(); 

                return RedirectToAction("Index");
            }

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == SelectedAddressId && a.UserId == userId);

            if (address == null)
                return RedirectToAction("Index");

           
            var order = new Order
            {
                UserId = userId,
                TotalAmount = totalAmount,
                ShippingCost = shippingCost,
                FinalAmount = finalAmount,
                DiscountAmount = 0,
                OrderStatus = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                CreatedAt = DateTime.Now,
                AddressId = address.Id,
                ReceiverName = address.ReceiverName,
                ReceiverPhone = address.ReceiverPhone,
                Province = address.Province,
                City = address.City,
                FullAddress = address.FullAddress,
                PostalCode = address.PostalCode
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cart.Items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = (decimal)item.Price
                };
                _context.OrderItems.Add(orderItem);
            }

            var deductionResult = await walletService.DeductAsync(
                userId,
                finalAmount,
                $"خرید سفارش #{order.Id}"
            );

            if (!deductionResult)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                TempData["Error"] = "خطا در کسر از کیف پول. لطفا مجدد تلاش کنید.";
                return RedirectToAction("Index");
            }

            order.PaymentStatus = PaymentStatus.Paid;
            order.OrderStatus = OrderStatus.Processing;

            _context.Orders.Update(order);
            string tranId = new Random().Next(100000, 999999).ToString();
            var payment = new Payment
            {
                Provider = "Wallet",
                Amount = finalAmount,
                IsSuccess = true,
                PaidAt = DateTime.Now,
                TransactionId = tranId,
                RefId = order.Id.ToString(),
                Description = $"پرداخت موفق برای سفارش #{order.Id} از طریق کیف پول"
            };
            _context.Payments.Add(payment);

            _context.CartItems.RemoveRange(cart.Items);

            await _context.SaveChangesAsync();
            return RedirectToAction("PaymentResult", new
            {
                msg = "پرداخت با موفقیت انجام شد! سفارش شما ثبت گردید.",
                finalAmount = finalAmount.ToString(), 
                orderId = order.Id
            });
        }



        [HttpGet]
        public async Task<IActionResult> InsufficientBalance()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Logout", "Account");
            }

            var userId = int.Parse(userIdString);

            var cart = await _cartService.GetOrCreateCartAsync();
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            decimal totalAmount = cart.Items.Sum(i => (decimal)i.Price * i.Quantity);
            decimal shippingCost = 0;
            decimal finalAmount = totalAmount + shippingCost;
            var walletService = HttpContext.RequestServices.GetService<IWalletService>();
            var walletBalance = await walletService.GetBalanceAsync(userId);

            var shortage = finalAmount - walletBalance;

            var vm = new InsufficientBalanceViewModel
            {
                WalletBalance = walletBalance,
                OrderAmount = finalAmount,
                Shortage = shortage,
                Cart = new CartViewModel
                {
                    Items = cart.Items.Select(i => new CartItemViewModel
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        Title = i.Product.Title,
                        ImageName = i.Product.MainImageName,
                        Price = (long)i.Price,
                        Quantity = i.Quantity
                    }).ToList()
                }
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> PaymentFailed()
        {
            var cart = await _cartService.GetOrCreateCartAsync();

 
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("PaymentResult", new
                {
                    msg = "سبد خرید شما خالی است!",
                    finalAmount = "0"
                });
            }

            decimal totalAmount = cart.Items.Sum(i => (decimal)i.Price * i.Quantity);
            decimal shippingCost = 0;
            decimal finalAmount = totalAmount + shippingCost;
            string tranId = new Random().Next(100000, 999999).ToString();


            var payment = new Payment
            {
                Provider = "Wallet",
                Amount = totalAmount,
                IsSuccess = false,
                PaidAt = DateTime.Now,
                TransactionId = tranId,
                RefId = null,
                Description = "پرداخت ناموفق - کاربر انصراف داد"
            };

            _context.Payments.Add(payment);


            await _context.SaveChangesAsync();

            return RedirectToAction("PaymentResult", new
            {
                msg = "پرداخت ناموفق بود. سفارش شما ثبت نشد.",
                finalAmount = finalAmount.ToString() 
            });
        }
    }
}