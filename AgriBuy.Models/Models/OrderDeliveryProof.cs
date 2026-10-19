using System;

namespace AgriBuy.Models.Models
{
    public class OrderDeliveryProof
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }
        public Guid UploadedByUserId { get; set; }
        public User? UploadedByUser { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}