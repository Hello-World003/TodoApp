using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TodoApp.Models;
using TodoApp.Models.Dtos;

namespace TodoApp.Data
{
    public class ApiDbContext : IdentityDbContext
    {
        
        public DbSet<ItemData> Items { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get;set; }

        public ApiDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}