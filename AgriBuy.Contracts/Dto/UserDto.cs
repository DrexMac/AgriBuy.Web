using System;

namespace AgriBuy.Contracts.Dto
{
    public class UserDto
    {
        public Guid Id { get; set; } 
        public string FirstName { get; set; } = string.Empty; 
        public string LastName { get; set; } = string.Empty; 
        public string EmailAddress { get; set; } = string.Empty; 
        public string Role { get; set; } = string.Empty; 

    }
}
