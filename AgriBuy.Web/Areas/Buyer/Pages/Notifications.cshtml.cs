using AgriBuy.EntityFramework;
using AgriBuy.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgriBuy.Web.Areas.Buyer.Pages
{
    public class NotificationsModel : PageModel
    {
        private readonly DefaultDbContext _context;
        public NotificationsModel(DefaultDbContext context) => _context = context;
        public List<Notification> Notifications { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!Guid.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
                return RedirectToPage("/Accounts/Login");

            Notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(100)
                .ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostMarkReadAsync(Guid notificationId)
        {
            if (!Guid.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
                return RedirectToPage("/Accounts/Login");

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}