using EventTicketBookingService.DataAccess;
using EventTicketBookingService.Models;
using EventTicketBookingService.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace EventTicketBookingService.Repository
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

        public async Task<int> GetNextIdAsync(CancellationToken cancellationToken = default)
        {
            if (!await _context.Bookings.AnyAsync(cancellationToken))
                return 1;

            return await _context.Bookings.MaxAsync(x => x.Id, cancellationToken) + 1;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
