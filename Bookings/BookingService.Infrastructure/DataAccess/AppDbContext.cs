using BookingService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.DataAccess
{
    public class AppDbContext : DbContext
    {
        public DbSet<Booking> Bookings => Set<Booking>();

        public AppDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
