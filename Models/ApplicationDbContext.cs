using Microsoft.EntityFrameworkCore;
using ShopInventory.Models;


namespace ShopInventory.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }

    }
}
