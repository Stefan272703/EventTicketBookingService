using EventTicketBookingService.Models;

namespace EventTicketBookingService.Interfaces
{
    public interface IEventStore
    {
        public bool TryGetEventById(int id, out Event? @event);
        public void AddEvent(Event @event);
        public void RemoveEvent(Event @event);
    }
}
