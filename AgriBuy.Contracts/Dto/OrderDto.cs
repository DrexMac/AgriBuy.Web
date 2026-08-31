using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgriBuy.Models.Enums;

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
        public bool IsDeleted { get; set; }
        public string? ORNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? PayDate { get; set; }
        public DateTime? FulfillmentDate { get; set; }
        public List<OrderItemDto>? OrderItems { get; set; }

        // ---- NEW ----
        public string? ShippingFullName { get; set; }
        public string? ShippingPhoneNumber { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ShippingCityProvince { get; set; }
        public string? ShippingPostalCode { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime? EstimatedDeliveryDate { get; set; }
        public bool IsSeenByBuyer { get; set; } = true;
    }
}