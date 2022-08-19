using HeavenofBooksWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace HeavenofBooksWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Category> Categories { get; set; }
    }
}
