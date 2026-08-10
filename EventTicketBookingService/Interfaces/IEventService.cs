using EventTicketBookingService.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketBookingService.Interfaces
{
    public interface IEventService
    {
        public PaginatedResultDTO<EventDTO> GetAllEvents(string title, 
            DateTime? from,
            DateTime? to,
            int page,
            int pageSize);
        public EventDTO? GetEventById(int id);
        public EventDTO? CreateEvent(Event createdEvent);
        public EventDTO UpdateEvent(int id, Event createdEvent);
        public EventDTO DeleteEvent(int id);
    }
}
