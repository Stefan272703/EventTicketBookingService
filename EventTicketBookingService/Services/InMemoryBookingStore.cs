using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using System.Collections.Concurrent;

namespace EventTicketBookingService.Services
{
    public class InMemoryBookingStore: IBookingTaskQueue
    {
        private readonly ConcurrentQueue<Booking> _queue = new();

        public void Enqueue(Booking booking)
        {
            _queue.Enqueue(booking);
        }

        public bool TryDequeue(out Booking booking)
        {
            return _queue.TryDequeue(out booking);
        }
    }
}
