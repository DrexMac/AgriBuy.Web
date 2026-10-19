using AgriBuy.EntityFramework;
using AgriBuy.Models.Enums;
using AgriBuy.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using AgriBuy.Services.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgriBuy.Web.Areas.Seller.Pages.Orders
{
    public class StatusModel : PageModel
    {
        private readonly DefaultDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IStoreNotificationService _notifications;

        public StatusModel(DefaultDbContext context, IWebHostEnvironment environment, IStoreNotificationService notifications)
        {
            _context = context;
            _environment = environment;
            _notifications = notifications;
        }

        public List<Order> Orders { get; set; } = new();

        // Store ID used for the Back to Store button
        public Guid? StoreId { get; set; }

        public async Task OnGetAsync(Guid? storeid = null)
        {
            // Keep the store ID so the page can navigate
            // back to the correct store.
            StoreId = storeid;

            if (storeid == null)
                return;

            Orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDeliveryProof)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .Where(o =>
                    o.StoreId == storeid &&
                    o.IsPaid)
                .OrderByDescending(o => o.PayDate)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(
            Guid orderId, OrderStatus newStatus, Guid storeid, IFormFile? proofImage)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (!Guid.TryParse(userIdString, out var userId))
                return RedirectToPage("/Accounts/Login");

            // Verify the signed-in user owns the store in the submitted URL.
            var ownsStore = await _context.Stores.AnyAsync(s => s.Id == storeid && s.UserId == userId);
            if (!ownsStore)
                return Forbid();

            if (!Enum.IsDefined(typeof(OrderStatus), newStatus))
                return BadRequest();

            var order = await _context.Orders
                .Include(o => o.OrderDeliveryProof)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.StoreId == storeid);

            if (order == null || !order.IsPaid)
                return RedirectToPage(new { storeid });

            if (newStatus == OrderStatus.Delivered && proofImage == null && order.OrderDeliveryProof == null)
            {
                TempData["StatusError"] = "Upload a delivery-proof photo before marking the order as delivered.";
                return RedirectToPage(new { storeid });
            }

            if (proofImage != null && proofImage.Length > 0)
            {
                const long maxFileSize = 5 * 1024 * 1024;
                var extension = Path.GetExtension(proofImage.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowedExtensions.Contains(extension) || proofImage.Length > maxFileSize)
                {
                    TempData["StatusError"] = "Please upload a JPG, PNG, or WEBP image up to 5 MB.";
                    return RedirectToPage(new { storeid });
                }

                var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "delivery-proofs");
                Directory.CreateDirectory(uploadFolder);
                var fileName = $"{Guid.NewGuid():N}{extension}";
                var fullPath = Path.Combine(uploadFolder, fileName);
                await using (var stream = System.IO.File.Create(fullPath))
                    await proofImage.CopyToAsync(stream);

                var relativePath = $"/uploads/delivery-proofs/{fileName}";
                if (order.OrderDeliveryProof == null)
                {
                    order.OrderDeliveryProof = new OrderDeliveryProof
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        UploadedByUserId = userId,
                        ImagePath = relativePath,
                        UploadedAt = DateTime.UtcNow
                    };
                }
                else
                {
                    order.OrderDeliveryProof.ImagePath = relativePath;
                    order.OrderDeliveryProof.UploadedByUserId = userId;
                    order.OrderDeliveryProof.UploadedAt = DateTime.UtcNow;
                }
            }

            order.Status = newStatus;
            order.FulfillmentDate = newStatus == OrderStatus.Delivered ? DateTime.UtcNow : null;
            await _context.SaveChangesAsync();

            if (newStatus == OrderStatus.Delivered)
            {
                try { await _notifications.NotifyOrderDeliveredAsync(order.Id); }
                catch (Exception) { /* The saved order status must not be undone by notification issues. */ }
            }

            return RedirectToPage(new { storeid });
        }
    }
}
