using EventTicketBookingService.Domain.Models;

namespace EventTicketBookingService.Application.Repository.Interface
{
    public interface IBookingRepository
    {
        public Task<Booking?> GetBookingByIdAsync(int id, CancellationToken cancellationToken = default);
        public Task<List<int>> GetBookingPendingIdsAsync(CancellationToken cancellationToken);
        public Task AddBookingAsync(Booking booking, CancellationToken cancellationToken = default);
        public Task SaveChangesAsync(CancellationToken cancellationToken = default);
        public Task<int> GetNextIdAsync(CancellationToken cancellationToken = default);
    }
}
