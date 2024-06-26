using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TodoApp.Models.Dtos
{
    public class RefreshToken
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public string JwtId { get; set; }
        public bool isUsed { get; set; }
        public bool isRevorked { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        
        [ForeignKey(nameof(UserId))]
        public IdentityUser User { get; set; }
    }
}