using AgriBuy.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriBuy.Models.Models
{
    public class Store
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid UserId { get; set; }
        public User? User { get; set; }
        public ICollection<Product>? Products { get; set; } 
    }

    public class StoreViewModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid UserId { get; set; } = Guid.Empty;
        public IEnumerable<ProductViewModel>? Products { get; set; }
    }
}