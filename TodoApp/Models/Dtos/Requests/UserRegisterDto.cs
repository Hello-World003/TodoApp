using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TodoApp.Models.Dtos.Requests
{
    public class UserRegisterDto
    {
        
        [Required]
        [NotNull]
        public string UserName { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }
        
        
        [Required]
        [MinLength(8)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}