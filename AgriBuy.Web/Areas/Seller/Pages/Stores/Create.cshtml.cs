using AgriBuy.Contracts;
using AgriBuy.Models.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AgriBuy.Web.Areas.Seller.Pages.Stores
{
    public class CreateModel : PageModel
    {
        private readonly IStoreService _storeService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;

        [BindProperty]
        public StoreViewModel Input { get; set; }

        public CreateModel(
            IStoreService storeService,
            IProductService productService,
            IOrderService orderService,
            IMapper mapper)
        {
            _storeService = storeService;
            _productService = productService;
            _orderService = orderService;
            _mapper = mapper;
        }

        public async Task<IActionResult> OnGet()
        {
            var orders = await _orderService.GetAllAsync();
            var ordersWithNumber = orders.Select(s => new Order
            {
                Id = s.Id,
                OrderNumber = string.IsNullOrEmpty(s.OrderNumber) ? "Unnamed Subject" : s.OrderNumber,
            }).ToList();

            ViewData["Orders"] = ordersWithNumber;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var userIdString = HttpContext.Session.GetString("UserId");
            if (!Guid.TryParse(userIdString, out var userId))
                return RedirectToPage("/Accounts/Login");

            Input.Id = Guid.NewGuid();
            Input.UserId = userId;

            await _storeService.AddAsync(Input);

            // Redirect to product creation, passing StoreId
            return RedirectToPage("/Products/Create", new { storeId = Input.Id });
        }


    }
}
