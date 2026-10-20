using EventTicketBookingService.Models;

namespace EventTicketBookingService.Repository.Interface
{
    public interface IBookingRepository
    {
        Task<Booking?> GetBookingByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddBookingAsync(Booking booking, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<int> GetNextIdAsync(CancellationToken cancellationToken = default);
    }
}
