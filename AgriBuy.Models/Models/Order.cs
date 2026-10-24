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

        // Shipping information
        public DateTime? EstimatedDeliveryDate { get; set; }
        public bool IsSeenByBuyer { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ShippingCityProvince { get; set; }
        public string? ShippingFullName { get; set; }
        public string? ShippingPhoneNumber { get; set; }
        public string? ShippingPostalCode { get; set; }

        // Payment and delivery status
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Path to the delivery photo uploaded by the seller.
        // This replaces the need for a separate delivery-proof table.
        public string? DeliveryImagePath { get; set; }

        // TEMPORARY: Keep this until we update all existing pages.
        public virtual OrderDeliveryProof? OrderDeliveryProof { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
            = new List<OrderItem>();
    }
}