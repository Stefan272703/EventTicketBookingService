using EventTicketBookingService.DataAccess;
using EventTicketBookingService.Exceptions;
using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using EventTicketBookingService.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.ComponentModel.Design;

namespace EventTicketBookingService.Services
{
    public class BookingService : IBookingService
    {
        private static readonly SemaphoreSlim _bookingLock = new(1, 1);
        private readonly IBookingRepository _bookingRepository;
        private readonly IEventRepository _eventRepository;
        //private readonly AppDbContext _context;
        public BookingService(//AppDbContext appDbContext,
                              IBookingRepository bookingRepository)
        {
            //_context = appDbContext;
            _bookingRepository = bookingRepository;
        }

        public async Task<BookingResponse> CreateBookingAsync(int eventId, CancellationToken cancellationToken = default)
        {
            await _bookingLock.WaitAsync(cancellationToken);
            try
            {
                // Получаем событие по Id
                var @event = await _eventRepository.GetEventByIdAsync(eventId, cancellationToken);
                if (@event == null)
                {
                    throw new ResourceNotFoundException($"Не удалось создать бронь к несуществующему событию с ID: {eventId}");
                }

                if (@event.TryReserveSeats())
                {
                    Booking booking = new Booking()
                    {
                        // ToDo: Сомнительное место, так как несколько раз используется контекст
                        //Id = await  /*_context.Bookings.AnyAsync(cancellationToken)*/ ? await _context.Bookings.MaxAsync(x => x.Id, cancellationToken) + 1 : 1,
                        Id = await _bookingRepository.GetNextIdAsync(cancellationToken),
                        EventId = eventId,
                        Status = BookingStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        ProcessedAt = null,
                    };

                    //await _context.Bookings.AddAsync(booking, cancellationToken);
                    //await _context.SaveChangesAsync(cancellationToken);

                    await _bookingRepository.AddBookingAsync(booking, cancellationToken);
                    await _bookingRepository.SaveChangesAsync(cancellationToken);

                    BookingResponse response = new BookingResponse()
                    {
                        Id = booking.Id,
                        EventId = booking.EventId,
                        Status = BookingStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                    };

                    return response;
                }
                throw new NoAvailableSeatsException("No available seats for this event");
            }
            finally
            {
                _bookingLock.Release();
            }
        }

        public async Task<Booking>? GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken = default)
        {
            var existingBooking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);//_context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
            if (existingBooking == null)
                throw new ResourceNotFoundException($"Бронь с ID: {bookingId} не найдена");
            return existingBooking;
        }
    }
}
