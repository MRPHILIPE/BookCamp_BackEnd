using bookcamp.Models;
using Microsoft.EntityFrameworkCore;

namespace bookcamp.Data
{
    public class BookCampDbContext : DbContext
    {
        public BookCampDbContext(DbContextOptions<BookCampDbContext> options) : base(options)
        {
            
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Camp> Camps { get; set; }
    }
}
