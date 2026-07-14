using EventTicketBookingService.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketBookingService.Interfaces
{
    public interface IEventService
    {
        public List<Event> GetAllEvents();
        public Event? GetEventById(int id);
        public Event? CreateEvent([FromBody] Event my_event /*string title, string description*/);
        public Event UpdateEvent(int id, [FromBody] Event my_event);
        public Event DeleteEvent(int id);
    }
}
