using AgriBuy.Contracts;
using AgriBuy.Contracts.Dto;
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

        public ShoppingCartModel(IShoppingCartService shoppingCartService)
        {
            _shoppingCartService = shoppingCartService;
        }

        public List<ShoppingCartDto> CartItems { get; set; } = new List<ShoppingCartDto>();
        public decimal OrderTotal { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return RedirectToPage("/Accounts/Login");

            CartItems = (await _shoppingCartService.GetByUserIdAsync(userId)).ToList();
            OrderTotal = CartItems.Sum(x => x.ItemPrice);
            return Page();
        }

        // Fixed Remove: only deletes the specific item
        public async Task<IActionResult> OnPostRemoveAsync(Guid cartItemId)
        {
            await _shoppingCartService.DeleteAsync(cartItemId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostIncreaseAsync(Guid cartItemId)
        {
            var item = CartItems.FirstOrDefault(c => c.Id == cartItemId);
            if (item != null)
            {
                item.Quantity++;
                item.ItemPrice = item.Quantity * item.UnitPrice;
                await _shoppingCartService.UpdateAsync(item);
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDecreaseAsync(Guid cartItemId)
        {
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
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostClearAsync()
        {
            var userId = GetCurrentUserId();
            await _shoppingCartService.ClearCartAsync(userId);
            return RedirectToPage();
        }

        private Guid GetCurrentUserId()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            return Guid.TryParse(userIdStr, out var userId) ? userId : Guid.Empty;
        }
    }
}
