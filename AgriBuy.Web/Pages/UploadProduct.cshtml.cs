using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AgriBuy.Contracts;
using System.Security.Claims;
using AgriBuy.Models.ViewModels;
using AgriBuy.Contracts.Dto;
using AutoMapper;

public class UploadProductModel : PageModel
{
    private readonly IProductService _productService;
    private readonly IMapper _mapper;

    public UploadProductModel(IProductService productService, IMapper mapper)
    {
        _productService = productService;
        _mapper = mapper;
    }

    [BindProperty]
    public ProductViewModel Product { get; set; } = new ProductViewModel();

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        Guid currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var productDto = _mapper.Map<ProductDto>(Product);
        await _productService.AddAsync(productDto, currentUserId);

        return RedirectToPage("/UserProfile/MyProducts");
    }
}
