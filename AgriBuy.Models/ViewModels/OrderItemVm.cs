using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriBuy.Models.ViewModels
{
    public class OrderItemVm
    {
        public string ProductName { get; set; } = null!;
        public string UnitOfMeasure { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal ItemPrice { get; set; }
    }
}
