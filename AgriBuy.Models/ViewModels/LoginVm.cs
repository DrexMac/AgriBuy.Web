using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AgriBuy.Models.ViewModels
{
    public class LoginVm
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; } = null!;
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
