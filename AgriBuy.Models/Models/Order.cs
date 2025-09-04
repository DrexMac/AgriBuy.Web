using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriBuy.Models.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public virtual User? User { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid StoreId { get; set; }
        public virtual Store? Store { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsPaid { get; set; }
        public string? ORNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? PayDate { get; set; }
        public DateTime? FulfillmentDate { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
