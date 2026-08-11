using EventTicketBookingService.Exceptions;
using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using System.ComponentModel.Design;

namespace EventTicketBookingService.Services
{
    public class BookingService: IBookingService
    {
        private List<Booking> _bookings = [];
        private readonly IBookingTaskQueue _taskQueue;
        public BookingService(IBookingTaskQueue taskQueue)
        {
            _taskQueue = taskQueue;
        }

        public async Task<BookingResponse> CreateBookingAsync(int eventId)
        {
            Booking booking = new Booking()
            {
                Id = _bookings.Any() ? _bookings.Max(x => x.Id) + 1 : 1,
                EventId = eventId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now,
                ProcessedAt = null,
            };

            _taskQueue.Enqueue(booking);

            // Маппим в DTO тело ответа
            BookingResponse response = new BookingResponse()
            {
                Id = booking.Id,
                EventId = booking.EventId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now
            };

            _bookings.Add(booking);

            return response;
        }

        public async Task<Booking>? GetBookingByIdAsync(int bookingId)
        {
            var existingBooking = _bookings.FirstOrDefault(x => x.Id == bookingId);

            return _bookings?.FirstOrDefault(x => x?.Id == bookingId);
        }
    }
}
