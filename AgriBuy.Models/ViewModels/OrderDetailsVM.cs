using AgriBuy.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgriBuy.Models.ViewModels
{
    public class OrderDetailsVM
    {
        public class OrderDetailsVm
        {
            public Guid Id { get; set; }
            public string OrderNumber { get; set; } = null!;
            public DateTime OrderDate { get; set; }
            public decimal TotalPrice { get; set; }
            public bool IsPaid { get; set; }
            public IEnumerable<OrderItemVm>? OrderItems { get; set; }
        }
    }
}
