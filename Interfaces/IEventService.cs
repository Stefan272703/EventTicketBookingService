using EventTicketBookingService.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketBookingService.Interfaces
{
    public interface IEventService
    {
        public List<EventDTO> GetAllEvents(string title, DateTime? from, DateTime? to);
        public EventDTO? GetEventById(int id);
        public EventDTO? CreateEvent(Event my_event);
        public EventDTO UpdateEvent(int id, Event my_event);
        public EventDTO DeleteEvent(int id);
    }
}
