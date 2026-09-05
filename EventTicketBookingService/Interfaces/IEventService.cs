using EventTicketBookingService.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketBookingService.Interfaces
{
    public interface IEventService
    {
        public PaginatedResultDTO<Event> GetAllEvents(string title, 
            DateTime? from,
            DateTime? to,
            int page,
            int pageSize);
        public Event? GetEventById(int id);
        public Task<EventInfo?>? CreateEventAsync(EventInfo createdEvent);
        public Event UpdateEvent(int id, EventInfo createdEvent);
        public Event DeleteEvent(int id);
    }
}
