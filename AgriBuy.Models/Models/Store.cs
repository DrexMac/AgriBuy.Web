using AgriBuy.Models.ViewModels;
using System;
using System.Collections.Generic;

namespace AgriBuy.Models.Models
{
    public class Store
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid UserId { get; set; }
        public User? User { get; set; }

        // existing relationship
        public ICollection<Product>? Products { get; set; }

        //  NEW: Add this relationship to Orders
        public ICollection<Order>? Orders { get; set; } = new List<Order>();
    }

    public class StoreViewModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid UserId { get; set; } = Guid.Empty;
        public IEnumerable<ProductViewModel>? Products { get; set; }

        // Optional: include Orders if you need to show them in a viewmodel too
        public IEnumerable<Order>? Orders { get; set; }
    }
}
