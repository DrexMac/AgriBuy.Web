
using AgriBuy.EntityFramework;
using AgriBuy.Models.Enums;
using AgriBuy.Models.Models;
using AgriBuy.Services.Notifications;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgriBuy.Web.Areas.Seller.Pages.Orders
{
    public class StatusModel : PageModel
    {
        private readonly DefaultDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IStoreNotificationService _notifications;
        private readonly ILogger<StatusModel> _logger;

        public StatusModel(
            DefaultDbContext context,
            IWebHostEnvironment environment,
            IStoreNotificationService notifications,
            ILogger<StatusModel> logger)
        {
            _context = context;
            _environment = environment;
            _notifications = notifications;
            _logger = logger;
        }

        public List<Order> Orders { get; set; } = new();

        // Store ID used for the Back to Store button.
        public Guid? StoreId { get; set; }

        public async Task OnGetAsync(Guid? storeid = null)
        {
            StoreId = storeid;

            if (storeid == null)
                return;

            Orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .Where(o => o.StoreId == storeid && o.IsPaid)
                .OrderByDescending(o => o.PayDate)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(
            Guid orderId,
            OrderStatus newStatus,
            Guid storeid,
            IFormFile? proofImage)
        {
            var userIdString = HttpContext.Session.GetString("UserId");

            if (!Guid.TryParse(userIdString, out var userId))
                return RedirectToPage("/Accounts/Login");

            // Verify that the signed-in user owns this store.
            var ownsStore = await _context.Stores
                .AnyAsync(s => s.Id == storeid && s.UserId == userId);

            if (!ownsStore)
                return Forbid();

            if (!Enum.IsDefined(typeof(OrderStatus), newStatus))
                return BadRequest();

            var order = await _context.Orders
                .FirstOrDefaultAsync(o =>
                    o.Id == orderId &&
                    o.StoreId == storeid);

            if (order == null || !order.IsPaid)
                return RedirectToPage(new { storeid });

            // Validate the uploaded image before changing the order.
            if (proofImage != null && proofImage.Length > 0)
            {
                const long maxFileSize = 5 * 1024 * 1024;

                var extension = Path.GetExtension(proofImage.FileName)
                    .ToLowerInvariant();

                var allowedExtensions = new[]
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                };

                if (!allowedExtensions.Contains(extension) ||
                    proofImage.Length > maxFileSize)
                {
                    TempData["StatusError"] =
                        "Please upload a JPG, PNG, or WEBP image up to 5 MB.";

                    return RedirectToPage(new { storeid });
                }
            }

            // A delivery photo is required before marking an order Delivered.
            // An existing photo can be reused if the seller doesn't upload
            // another one.
            bool hasNewImage = proofImage != null && proofImage.Length > 0;

            if (newStatus == OrderStatus.Delivered &&
                !hasNewImage &&
                string.IsNullOrWhiteSpace(order.DeliveryImagePath))
            {
                TempData["StatusError"] =
                    "Upload a delivery-proof photo before marking the order as delivered.";

                return RedirectToPage(new { storeid });
            }

            string? newImagePath = null;
            string? newImageFullPath = null;

            try
            {
                // Save the uploaded file and store its web path on the order.
                if (hasNewImage)
                {
                    var webRootPath = _environment.WebRootPath;

                    if (string.IsNullOrWhiteSpace(webRootPath))
                    {
                        webRootPath = Path.Combine(
                            _environment.ContentRootPath,
                            "wwwroot");
                    }

                    var uploadFolder = Path.Combine(
                        webRootPath,
                        "uploads",
                        "delivery-proofs");

                    Directory.CreateDirectory(uploadFolder);

                    // Generate a safe filename instead of using the
                    // original filename supplied by the user.
                    var fileName = $"{Guid.NewGuid():N}" +
                        Path.GetExtension(proofImage!.FileName)
                            .ToLowerInvariant();

                    newImageFullPath = Path.Combine(uploadFolder, fileName);

                    await using (var stream = System.IO.File.Create(newImageFullPath))
                    {
                        await proofImage.CopyToAsync(stream);
                    }

                    newImagePath = $"/uploads/delivery-proofs/{fileName}";
                    order.DeliveryImagePath = newImagePath;
                }

                order.Status = newStatus;

                order.FulfillmentDate =
                    newStatus == OrderStatus.Delivered
                        ? DateTime.UtcNow
                        : null;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(
                    ex,
                    "Concurrency error while updating order {OrderId}.",
                    orderId);

                // Remove the newly uploaded file if saving the order failed.
                if (!string.IsNullOrWhiteSpace(newImageFullPath) &&
                    System.IO.File.Exists(newImageFullPath))
                {
                    try
                    {
                        System.IO.File.Delete(newImageFullPath);
                    }
                    catch (Exception fileException)
                    {
                        _logger.LogWarning(
                            fileException,
                            "Could not remove unused delivery image {ImagePath}.",
                            newImageFullPath);
                    }
                }

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while updating order {OrderId}.",
                    orderId);

                // Avoid leaving an unused image if the database save failed.
                if (!string.IsNullOrWhiteSpace(newImageFullPath) &&
                    System.IO.File.Exists(newImageFullPath))
                {
                    try
                    {
                        System.IO.File.Delete(newImageFullPath);
                    }
                    catch (Exception fileException)
                    {
                        _logger.LogWarning(
                            fileException,
                            "Could not remove unused delivery image {ImagePath}.",
                            newImageFullPath);
                    }
                }

                throw;
            }

            // Notify the buyer only after the order update has been saved.
            if (newStatus == OrderStatus.Delivered)
            {
                try
                {
                    await _notifications.NotifyOrderDeliveredAsync(order.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Order {OrderId} was updated, but the delivery notification failed.",
                        order.Id);
                }
            }

            return RedirectToPage(new { storeid });
        }
    }
}
