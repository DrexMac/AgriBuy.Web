using AgriBuy.Contracts;
using AgriBuy.Contracts.Dto;
using AgriBuy.Services.Checkout;
using AgriBuy.EntityFramework;
using AgriBuy.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgriBuy.Web.Areas.Buyer.Pages
{
    public class ShoppingCartModel : PageModel
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICheckoutService _checkoutService;
        private readonly DefaultDbContext _context;

        public ShoppingCartModel(IShoppingCartService shoppingCartService, ICheckoutService checkoutService, DefaultDbContext context)
        {
            _shoppingCartService = shoppingCartService;
            _checkoutService = checkoutService;
            _context = context;
        }

        public List<ShoppingCartDto> CartItems { get; set; } = new();
        public decimal OrderTotal { get; set; }
        public List<Order> RecentPaidOrders { get; set; } = new();

        private Guid GetCurrentUserId()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            return Guid.TryParse(userIdStr, out var userId) ? userId : Guid.Empty;
        }

        private async Task LoadCartAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                CartItems = new();
                OrderTotal = 0;
                return;
            }

            CartItems = (await _shoppingCartService.GetByUserIdAsync(userId)).ToList();
            OrderTotal = CartItems.Sum(x => x.ItemPrice);
        }

        private async Task LoadRecentPaidOrdersAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return;

            RecentPaidOrders = await _context.Orders
                .Where(o => o.UserId == userId && o.IsPaid)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.PayDate)
                .Take(3)
                .ToListAsync();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadCartAsync();
            await LoadRecentPaidOrdersAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostRemoveAsync(Guid cartItemId)
        {
            await _shoppingCartService.DeleteAsync(cartItemId);
            await LoadCartAsync();
            await LoadRecentPaidOrdersAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostIncreaseAsync(Guid cartItemId)
        {
            await LoadCartAsync();
            var item = CartItems.FirstOrDefault(c => c.Id == cartItemId);
            if (item != null)
            {
                item.Quantity++;
                item.ItemPrice = item.Quantity * item.UnitPrice;
                await _shoppingCartService.UpdateAsync(item);
            }
            await LoadCartAsync();
            await LoadRecentPaidOrdersAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDecreaseAsync(Guid cartItemId)
        {
            await LoadCartAsync();
            var item = CartItems.FirstOrDefault(c => c.Id == cartItemId);
            if (item != null)
            {
                if (item.Quantity <= 1)
                    await _shoppingCartService.DeleteAsync(cartItemId);
                else
                {
                    item.Quantity--;
                    item.ItemPrice = item.Quantity * item.UnitPrice;
                    await _shoppingCartService.UpdateAsync(item);
                }
            }
            await LoadCartAsync();
            await LoadRecentPaidOrdersAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostClearAsync()
        {
            var userId = GetCurrentUserId();
            await _shoppingCartService.ClearCartAsync(userId);
            await LoadCartAsync();
            await LoadRecentPaidOrdersAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCheckoutAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return RedirectToPage("/Login");

            var successUrlString = Url.Page(
                "/Cart/Success",
                pageHandler: null,
                values: new { area = "Buyer" },
                protocol: Request.Scheme);

            var failureUrlString = Url.Page(
                "/Cart/Failed",
                pageHandler: null,
                values: new { area = "Buyer" },
                protocol: Request.Scheme);

            var successUrl = new Uri(successUrlString);
            var failureUrl = new Uri(failureUrlString);

            var redirectUrl = await _checkoutService.CreateCheckoutAndGetRedirectUrlAsync(
                userId,
                successUrl,
                failureUrl
            );

            if (redirectUrl.Contains("Success", StringComparison.OrdinalIgnoreCase))
            {
                await _shoppingCartService.ClearCartAsync(userId);
                return RedirectToPage("/ShoppingCart", new { area = "Buyer" });
            }

            return Redirect(redirectUrl);
        }
    }
}
