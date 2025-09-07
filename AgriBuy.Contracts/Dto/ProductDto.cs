using AgriBuy.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriBuy.Contracts.Dto
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public Guid StoreId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string UnitOfMeasure { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public bool IsAvailable { get; set; }
        public Guid UserId { get; set; }
        public string? ImagePath { get; set; }
    }

}
