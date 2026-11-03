using EventTicketBookingService.Infrastructure.DataAccess;
using EventTicketBookingService.Domain.Models;
using EventTicketBookingService.Application.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace EventTicketBookingService.Infrastructure.Repository
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;
        public BookingRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task AddBookingAsync(Booking booking, CancellationToken cancellationToken = default)
        {
            return _context.Bookings.AddAsync(booking, cancellationToken).AsTask();
        }

        public Task<Booking?> GetBookingByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _context.Bookings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public Task<List<int>> GetBookingPendingIdsAsync(CancellationToken cancellationToken)
        {
            return _context.Bookings
                    .Where(b => b.Status == BookingStatus.Pending)
                    .Select(b => b.Id).
                    ToListAsync(cancellationToken);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
