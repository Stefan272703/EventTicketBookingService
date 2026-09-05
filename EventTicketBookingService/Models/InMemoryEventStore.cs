using EventTicketBookingService.Interfaces;
using System.Collections.Concurrent;

namespace EventTicketBookingService.Models
{
    public class InMemoryEventStore: IEventStore
    {
        private readonly ConcurrentDictionary<int, Event> _eventStore = new();

        public bool TryGetEventById(int id, out Event? @event)
        {
            @event = null;
            if (_eventStore.TryGetValue(id, out var foundEvent))
            {
                @event = foundEvent;
                return true;
            }
            return false;
        }

        public void AddEvent(Event @event)
        {
            _eventStore.TryAdd(@event.Id, @event);
        }
        public void RemoveEvent(Event @event) 
        {
            _eventStore.TryRemove(@event.Id, out _);

        }
    }
}
