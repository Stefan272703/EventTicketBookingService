using EventTicketBookingService.Exceptions;
using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using System.Collections.Concurrent;
using System.ComponentModel.Design;

namespace EventTicketBookingService.Services
{
    public class BookingService: IBookingService
    {
        private readonly object _bookingLock = new();
        private ConcurrentDictionary<int, Booking> _bookings = [];
        private readonly IBookingTaskQueue _taskQueue;
        private readonly IEventService _eventService;
        public BookingService(IBookingTaskQueue taskQueue,
                              IEventService eventService)
        {
            _taskQueue = taskQueue;
            _eventService = eventService;
        }

        public async Task<BookingResponse> CreateBookingAsync(int eventId)
        {
            lock (_bookingLock)
            {
                var eventById = _eventService.GetEventById(eventId);
                if (eventById == null)
                    throw new ResourceNotFoundException(eventById, $"Не найдено событие по ID: {eventId}");

                if (eventById.TryReserveSeats())
                {
                    Booking booking = new Booking()
                    {
                        Id = _bookings.Any() ? _bookings.Max(x => x.Key) + 1 : 1,
                        EventId = eventId,
                        Status = BookingStatus.Pending,
                        CreatedAt = DateTime.Now,
                        ProcessedAt = null,
                    };

                    _taskQueue.Enqueue(booking);
                    _bookings.TryAdd(booking.Id, booking);

                    BookingResponse response = new BookingResponse()
                    {
                        Id = booking.Id,
                        EventId = booking.EventId,
                        Status = BookingStatus.Pending,
                        CreatedAt = DateTime.Now
                    };

                    return response;
                }
                else
                {
                    throw new NoAvailableSeatsException("No available seats for this event");
                }
            }
        }

        public async Task<Booking>? GetBookingByIdAsync(int bookingId)
        {
            var existingBooking = _bookings.FirstOrDefault(x => x.Key == bookingId);
            if (existingBooking.Value == null)
                throw new ResourceNotFoundException($"Бронь с ID: {bookingId} не найдена");
            return existingBooking.Value;
        }

        public async Task UpdateBookingStatusAsync(int bookingId, BookingStatus status, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            var booking = _bookings.FirstOrDefault(x => x.Key == bookingId);
            
            if(booking.Value == null)
                throw new ResourceNotFoundException($"Бронь с ID: {bookingId} не найдена");

            booking.Value.Status = status;
            if(status == BookingStatus.Confirmed || status == BookingStatus.Rejected)
                booking.Value.ProcessedAt = DateTime.Now;

        }
    }
}
