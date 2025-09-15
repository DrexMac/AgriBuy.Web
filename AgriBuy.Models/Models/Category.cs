using System;
using System.Collections.Generic;

namespace AgriBuy.Models.Models
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;

      
        public Guid? ParentId { get; set; }
        public Category? Parent { get; set; }
        public ICollection<Category>? Children { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
