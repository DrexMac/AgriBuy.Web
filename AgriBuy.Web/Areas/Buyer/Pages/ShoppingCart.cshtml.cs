using AgriBuy.Contracts;
using AgriBuy.Contracts.Dto;
using AgriBuy.Services.Checkout;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        public ShoppingCartModel(IShoppingCartService shoppingCartService, ICheckoutService checkoutService)
        {
            _shoppingCartService = shoppingCartService;
            _checkoutService = checkoutService;
        }

        public List<ShoppingCartDto> CartItems { get; set; } = new List<ShoppingCartDto>();
        public decimal OrderTotal { get; set; }

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
                CartItems = new List<ShoppingCartDto>();
                OrderTotal = 0;
            }
            else
            {
                CartItems = (await _shoppingCartService.GetByUserIdAsync(userId)).ToList();
                OrderTotal = CartItems.Sum(x => x.ItemPrice);
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadCartAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostRemoveAsync(Guid cartItemId)
        {
            await _shoppingCartService.DeleteAsync(cartItemId);
            await LoadCartAsync();
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
            return Page();
        }

        public async Task<IActionResult> OnPostClearAsync()
        {
            var userId = GetCurrentUserId();
            await _shoppingCartService.ClearCartAsync(userId);
            await LoadCartAsync();
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

            if (string.IsNullOrEmpty(successUrlString) || string.IsNullOrEmpty(failureUrlString))
                throw new Exception("Unable to generate checkout redirect URLs.");

            var successUrl = new Uri(successUrlString);
            var failureUrl = new Uri(failureUrlString);

            var redirectUrl = await _checkoutService.CreateCheckoutAndGetRedirectUrlAsync(
                userId,
                successUrl,
                failureUrl
            );

            return Redirect(redirectUrl);
        }


    }
}
