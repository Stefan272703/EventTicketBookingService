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
        public Event? CreateEvent(EventDTO createdEvent);
        public Event UpdateEvent(int id, EventDTO createdEvent);
        public Event DeleteEvent(int id);
    }
}
