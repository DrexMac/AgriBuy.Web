using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriBuy.Models.Models
{
    public class Product
    {
        public Guid Id { get; set; }

        // You store the owner user id on the product as well (keeps queries straightforward)
        public Guid UserId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string UnitOfMeasure { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsAvailable { get; set; }

        public Guid StoreId { get; set; }
        public Store? Store { get; set; }

        public string ImagePath { get; set; } = string.Empty;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}