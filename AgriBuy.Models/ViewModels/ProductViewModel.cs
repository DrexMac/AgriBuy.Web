using AgriBuy.Models.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AgriBuy.Models.ViewModels
{
    public class ProductViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        public string UnitOfMeasure { get; set; } = null!;

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public Guid StoreId { get; set; }

        public Product? Product { get; set; }
        public List<string> Stores { get; set; } = new();
        public List<Guid> StoreIds { get; set; } = new();

        public IFormFile? UploadedImage { get; set; }
    }
}
