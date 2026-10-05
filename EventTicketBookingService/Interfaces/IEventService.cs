using EventTicketBookingService.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketBookingService.Interfaces
{
    public interface IEventService
    {
        public Task<PaginatedResultDTO<EventInfo>> GetAllEventsAsync(string title,
            DateTime? from,
            DateTime? to,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);
        public Task<EventInfo?> GetEventByIdAsync(int id, CancellationToken cancellationToken = default);
        public Task<EventInfo?>? CreateEventAsync(EventInfo createdEvent, CancellationToken cancellationToken = default);
        public Task<Event> UpdateEventAsync(int id, EventInfo createdEvent, CancellationToken cancellationToken = default);
        public Task<Event> DeleteEventAsync(int id, CancellationToken cancellationToken = default);
    }
}
