using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AgriBuy.Contracts;
using AgriBuy.Contracts.Dto;

namespace AgriBuy.Web.Areas.Buyer.Pages
{
    public class ShoppingCartModel : PageModel
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;

        public ShoppingCartModel(
            IShoppingCartService shoppingCartService,
            IUserService userService,
            IOrderService orderService)
        {
            _shoppingCartService = shoppingCartService;
            _userService = userService;
            _orderService = orderService;
        }

        public IEnumerable<ShoppingCartDto> CartItems { get; set; } = Enumerable.Empty<ShoppingCartDto>();
        public decimal OrderTotal { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return RedirectToPage("/Accounts/Login");

            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
                return RedirectToPage("/Accounts/Login");

            var cartItems = await _shoppingCartService.GetByUserIdAsync(userId);
            CartItems = cartItems.ToList();

            if (!CartItems.Any())
                ErrorMessage = "Your cart is currently empty.";

            OrderTotal = CartItems.Sum(x => x.ItemPrice);
            return Page();
        }

        public async Task<IActionResult> OnPostCheckoutAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return RedirectToPage("/Accounts/Login");

            var cartItems = await _shoppingCartService.GetByUserIdAsync(userId);
            if (!cartItems.Any())
            {
                ErrorMessage = "Your cart is empty. Cannot proceed to checkout.";
                return Page();
            }

            var orderDto = new OrderDto
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OrderNumber = $"ORD-{DateTime.UtcNow.Ticks}",
                OrderDate = DateTime.UtcNow,
                TotalPrice = cartItems.Sum(ci => ci.ItemPrice),
                IsPaid = false,
                OrderItems = cartItems.Select(ci => new OrderItemDto
                {
                    Id = Guid.NewGuid(),
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice,
                    ItemPrice = ci.ItemPrice,
                    UnitOfMeasure = ci.UnitOfMeasure,
                    OrderId = Guid.Empty
                }).ToList()
            };

            await _orderService.AddAsync(orderDto);
            await _shoppingCartService.ClearCartAsync(userId);

            return RedirectToPage("/Buyer/Dashboard");
        }

        public async Task<IActionResult> OnPostAddOrderAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return RedirectToPage("/Accounts/Login");

            var cartItems = await _shoppingCartService.GetByUserIdAsync(userId);
            if (!cartItems.Any())
            {
                ErrorMessage = "Your cart is empty. Cannot add order.";
                return Page();
            }

            var orderDto = new OrderDto
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OrderNumber = $"ORD-{DateTime.UtcNow.Ticks}",
                OrderDate = DateTime.UtcNow,
                TotalPrice = cartItems.Sum(ci => ci.ItemPrice),
                IsPaid = false,
                OrderItems = cartItems.Select(ci => new OrderItemDto
                {
                    Id = Guid.NewGuid(),
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice,
                    ItemPrice = ci.ItemPrice,
                    UnitOfMeasure = ci.UnitOfMeasure,
                    OrderId = Guid.Empty
                }).ToList()
            };

            await _orderService.AddAsync(orderDto);

            // Optionally clear cart here or keep it as is
            // await _shoppingCartService.ClearCartAsync(userId);

            return RedirectToPage("/Buyer/Dashboard");
        }

        public async Task<IActionResult> OnPostIncreaseAsync(Guid cartItemId)
        {
            var userId = GetCurrentUserId();
            var cartItems = await _shoppingCartService.GetByUserIdAsync(userId);
            var item = cartItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (item == null) return NotFound();

            item.Quantity++;
            item.ItemPrice = item.Quantity * item.UnitPrice;

            await _shoppingCartService.UpdateAsync(item);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDecreaseAsync(Guid cartItemId)
        {
            var userId = GetCurrentUserId();
            var cartItems = await _shoppingCartService.GetByUserIdAsync(userId);
            var item = cartItems.FirstOrDefault(ci => ci.Id == cartItemId);
            if (item == null) return NotFound();

            if (item.Quantity <= 1)
            {
                await _shoppingCartService.DeleteAsync(cartItemId);
            }
            else
            {
                item.Quantity--;
                item.ItemPrice = item.Quantity * item.UnitPrice;
                await _shoppingCartService.UpdateAsync(item);
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid cartItemId)
        {
            await _shoppingCartService.DeleteAsync(cartItemId);
            return RedirectToPage();
        }

        private Guid GetCurrentUserId()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            return Guid.TryParse(userIdStr, out var userId) ? userId : Guid.Empty;
        }
    }
}