using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriBuy.Contracts.Dto
{
    public class ShoppingCartDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }

        // Display info
        public string ProductName { get; set; } = null!;
        public string? ImagePath { get; set; }
        public string UnitOfMeasure { get; set; } = null!;
        public string? Description { get; set; }

        // Pricing
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal ItemPrice { get; set; }
    }
}
