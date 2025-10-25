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

            // Get all paid orders for this user, newest first
            var orders = (await _orderService.GetByUserIdAsync(userId))
                .Where(o => o.IsPaid)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            if (!orders.Any())
                return Page();

            // Flatten all order items with store info
            var storeGroups = new Dictionary<Guid, GroupedOrderVm>();

            foreach (var order in orders)
            {
                var orderItems = order.OrderItems?.ToList() ?? new List<OrderItemDto>();
                foreach (var item in orderItems)
                {
                    var product = await _productService.GetByIdAsync(item.ProductId);
                    var store = await _storeService.GetByIdAsync(order.StoreId);
                    var storeName = store?.Name ?? "AgriBuy Store";

                    if (!storeGroups.ContainsKey(order.StoreId))
                    {
                        storeGroups[order.StoreId] = new GroupedOrderVm
                        {
                            StoreName = storeName,
                            Items = new List<OrderItemVm>()
                        };
                    }

                    storeGroups[order.StoreId].Items.Add(new OrderItemVm
                    {
                        ProductName = product?.Name ?? $"Product #{item.ProductId}",
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        ItemPrice = item.ItemPrice
                    });
                }
            }

            // Compute totals per store
            foreach (var group in storeGroups.Values)
            {
                group.Total = group.Items.Sum(i => i.ItemPrice);
                GroupedOrders.Add(group);
            }

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
