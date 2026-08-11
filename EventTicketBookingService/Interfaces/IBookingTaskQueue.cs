using EventTicketBookingService.Models;

namespace EventTicketBookingService.Interfaces
{
    public interface IBookingTaskQueue
    {
        public void Enqueue(Booking booking);

        public bool TryDequeue(out Booking booking);
    }
}
