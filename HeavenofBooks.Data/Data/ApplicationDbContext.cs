using HeavenofBooks.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HeavenofBooks.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<CoverType> coverTypes { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<AppUser> appUser { get; set; }
        public DbSet<AppCompany> Companies { get; set; }
    }
}
