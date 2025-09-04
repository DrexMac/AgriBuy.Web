using AgriBuy.Models.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AgriBuy.Models.ViewModels
{
    public class ProductViewModel
    {
        public Guid ProductId { get; set; } = Guid.NewGuid();

        [Required]
        [DisplayName("Product Name")]
        public string? Name { get; set; }

        [Required]
        [DisplayName("Unit of Measure")]
        public string? UnitOfMeasure { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "UnitPrice must be greater than zero.")]
        public decimal UnitPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
        public int Quantity { get; set; }

        [DisplayName("Description")]
        public string? Description { get; set; }

        [DisplayName("Product Image")]
        public IFormFile? Image { get; set; }

        public IEnumerable<Guid>? OrderIds { get; set; }
    }
}