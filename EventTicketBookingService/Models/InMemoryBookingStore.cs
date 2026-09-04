using EventTicketBookingService.Interfaces;
using System.Collections.Concurrent;

namespace EventTicketBookingService.Models
{
    public class InMemoryBookingStore: IBookingTaskQueue
    {
        private readonly ConcurrentQueue<int> _queue = new();

        private readonly ConcurrentDictionary<int, Booking> _bookingStore = new();

        public void Enqueue(Booking booking)
        {
            if (_bookingStore.TryAdd(booking.Id, booking))
            {
                _queue.Enqueue(booking.Id);
            }
            else
            {
                return;
            }
        }

        public bool TryDequeue(out Booking? booking)
        {
            booking = null;

            if (!_queue.TryDequeue(out var id))
                return false;

            if (_bookingStore.TryRemove(id, out var foundBooking))
            {
                booking = foundBooking;
                return true;
            }
            return false;
        }

        public IEnumerable<Booking> GetPending()
        {
            return _bookingStore.Values.Where(b => b.Status == BookingStatus.Pending);
        }

        public void Update(Booking booking)
        {
            _bookingStore[booking.Id] = booking;
        }
    }
}
