using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AgriBuy.Models.ViewModels
{
    public class ProfileUpdateModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;
    }
}
