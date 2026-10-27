using EventTicketBookingService.Application.DTO;
using EventTicketBookingService.Domain.Models;

namespace EventTicketBookingService.Application.Repository.Interface
{
    public interface IEventRepository
    {
        public Task<Event?> GetEventByIdAsync(int id, CancellationToken cancellationToken = default);
        public Task<int> GetNextIdAsync(CancellationToken cancellationToken = default);
        public Task AddEventAsync(Event @event, CancellationToken cancellationToken = default);
        public Task<Event?> UpdateEventByIdAsync(int id, EventInfo eventInfo, CancellationToken cancellationToken = default);
        public Task<Event?> RemoveEventByIdAsync(int id, CancellationToken cancellationToken = default);

        public Task<(IReadOnlyList<Event?> items, int totalCount)> GetFilteredEvents(string title,
            DateTime? from,
            DateTime? to,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);
        public Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
