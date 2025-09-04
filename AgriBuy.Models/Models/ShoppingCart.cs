using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgriBuy.Models.Models
{
    public class ShoppingCart
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }

}



