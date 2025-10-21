using AgriBuy.Contracts;
using AgriBuy.Contracts.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgriBuy.Web.Areas.Buyer.Pages.Cart
{
    public class SuccessModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IOrderItemService _orderItemService;
        private readonly IProductService _productService;
        private readonly IStoreService _storeService;

        public SuccessModel(
            IOrderService orderService,
            IOrderItemService orderItemService,
            IProductService productService,
            IStoreService storeService)
        {
            _orderService = orderService;
            _orderItemService = orderItemService;
            _productService = productService;
            _storeService = storeService;
        }

        public List<GroupedOrderVm> GroupedOrders { get; set; } = new();

        private Guid GetCurrentUserId()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            return Guid.TryParse(userIdStr, out var userId) ? userId : Guid.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return RedirectToPage("/Accounts/Login");

            // ? Get all orders for this user, newest first
            var orders = (await _orderService.GetByUserIdAsync(userId))
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            if (!orders.Any())
                return Page();

            // ? Find the latest PAID order
            var latestOrder = orders.FirstOrDefault(o => o.IsPaid) ?? orders.First();

            // ? Load items for this order
            var orderItems = latestOrder.OrderItems?.ToList() ?? new List<OrderItemDto>();
            if (!orderItems.Any())
                return Page();

            // ? Group order items by store
            var store = await _storeService.GetByIdAsync(latestOrder.StoreId);
            var storeName = store?.Name ?? "AgriBuy Store";

            var itemsWithNames = new List<OrderItemVm>();
            foreach (var item in orderItems)
            {
                var product = await _productService.GetByIdAsync(item.ProductId);
                itemsWithNames.Add(new OrderItemVm
                {
                    ProductName = product?.Name ?? $"Product #{item.ProductId}",
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    ItemPrice = item.ItemPrice
                });
            }

            GroupedOrders.Add(new GroupedOrderVm
            {
                StoreName = storeName,
                Items = itemsWithNames,
                Total = itemsWithNames.Sum(i => i.ItemPrice)
            });

            return Page();
        }

        public class GroupedOrderVm
        {
            public string StoreName { get; set; } = string.Empty;
            public List<OrderItemVm> Items { get; set; } = new();
            public decimal Total { get; set; }
        }

        public class OrderItemVm
        {
            public string ProductName { get; set; } = string.Empty;
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
            public decimal ItemPrice { get; set; }
        }
    }
}
