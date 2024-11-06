using Models;
using Microsoft.EntityFrameworkCore;
using INTERFACES; // For repository interfaces

namespace DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserFile> UserFiles { get; set; }
    }
}
