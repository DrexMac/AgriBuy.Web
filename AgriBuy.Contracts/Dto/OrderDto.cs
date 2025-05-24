using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriBuy.Contracts.Dto
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = null!;
        public Guid UserId { get; set; }
        public Guid StoreId { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsPaid { get; set; }
        public string? ORNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? PayDate { get; set; }
        public DateTime? FulfillmentDate { get; set; }
        public List<OrderItemDto>? OrderItems { get; set; }
    }
}

