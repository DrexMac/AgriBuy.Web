using AgriBuy.Models.BaseModelFolder;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriBuy.Models.Models
{
    public class User : BaseModel
    {
        [Required]
        [MaxLength(100)]
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public UserRole Role { get; set; }
        public int Points { get; set; }
        public bool IsDeleted { get; set; }
        public bool? IsActive { get; set; }
        public ICollection<LoginInfo> LoginInfos { get; set; } = [];
        public ICollection<Order> Orders { get; set; } = [];
        public string? PasswordHash { get; set; }
        public Store? Store { get; set; }
    }
}
