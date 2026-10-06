using EventTicketBookingService.DataAccess;
using EventTicketBookingService.Exceptions;
using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.ComponentModel.Design;

namespace EventTicketBookingService.Services
{
    public class BookingService : IBookingService
    {
        //private readonly object _bookingLock = new();
        private readonly SemaphoreSlim _bookingLock = new(1, 1);
        private ConcurrentDictionary<int, Booking> _bookings = [];
        //private readonly IBookingTaskQueue _taskQueue;
        //private readonly IEventStore _eventStore;
        private readonly AppDbContext _context;
        public BookingService(AppDbContext appDbContext)
            //IBookingTaskQueue taskQueue,
                              //IEventStore eventStore)
        {
            _context = appDbContext;
            //_taskQueue = taskQueue;
            //_eventStore = eventStore;
        }

        public async Task<BookingResponse> CreateBookingAsync(int eventId, CancellationToken cancellationToken = default)
        {
            await _bookingLock.WaitAsync(cancellationToken);
            //lock (_bookingLock)
            //{
            try
            {
                // Получаем первое событие по Id
                var @event = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);
                if (@event == null)
                {
                    throw new ResourceNotFoundException($"Не удалось создать бронь к несуществующему событию с ID: {eventId}");
                }

                //if (!_eventStore.TryGetEventById(eventId, out var @event))
                //{
                //    throw new ResourceNotFoundException($"Не удалось создать бронь к несуществующему событию с ID: {eventId}");
                //}

                if (@event.TryReserveSeats())
                {
                    Booking booking = new Booking()
                    {
                        Id = _bookings.Any() ? _bookings.Max(x => x.Key) + 1 : 1,
                        EventId = eventId,
                        Status = BookingStatus.Pending,
                        CreatedAt = DateTime.Now,
                        ProcessedAt = null,
                    };

                    //_taskQueue.Enqueue(booking);
                    //_bookings.TryAdd(booking.Id, booking);
                    await _context.Bookings.AddAsync(booking, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    BookingResponse response = new BookingResponse()
                    {
                        Id = booking.Id,
                        EventId = booking.EventId,
                        Status = BookingStatus.Pending,
                        CreatedAt = DateTime.Now
                    };

                    return response;
                }
                throw new NoAvailableSeatsException("No available seats for this event");
            }
            finally
            {
                _bookingLock.Release();
            }
            //}
        }

        public async Task<Booking>? GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken = default)
        {
            //var existingBooking = _bookings.FirstOrDefault(x => x.Key == bookingId);
            var existingBooking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
            if (existingBooking == null)
                throw new ResourceNotFoundException($"Бронь с ID: {bookingId} не найдена");
            return existingBooking;
        }

        //public async Task UpdateBookingStatusAsync(int bookingId, BookingStatus status, CancellationToken cancellationToken = default)
        //{
        //    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        //    var booking = _bookings.FirstOrDefault(x => x.Key == bookingId);

        //    if (booking.Value == null)
        //        throw new ResourceNotFoundException($"Бронь с ID: {bookingId} не найдена");

        //    booking.Value.Status = status;
        //    if (status == BookingStatus.Confirmed || status == BookingStatus.Rejected)
        //        booking.Value.ProcessedAt = DateTime.Now;
        //}

    }
}
