using AgriBuy.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgriBuy.Models.ViewModels
{
    public class ShoppingCartVm
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string UnitOfMeasure { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal ItemPrice { get; set; }
    }
}
