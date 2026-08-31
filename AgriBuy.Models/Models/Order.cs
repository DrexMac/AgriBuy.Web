using AgriBuy.Models.Enums;
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

        // ---- NEW: Shipping & order tracking ----
        public string? ShippingFullName { get; set; }
        public string? ShippingPhoneNumber { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ShippingCityProvince { get; set; }
        public string? ShippingPostalCode { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime? EstimatedDeliveryDate { get; set; }

        // True = buyer has seen the latest status. Set to false whenever
        // the seller changes the status, so the buyer sees an "Updated!" badge.
        public bool IsSeenByBuyer { get; set; } = true;
    }
}