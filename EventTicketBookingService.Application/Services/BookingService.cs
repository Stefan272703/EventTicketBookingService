using EventTicketBookingService.Domain.Exceptions;
using EventTicketBookingService.Application.Interfaces;
using EventTicketBookingService.Application.DTO;
using EventTicketBookingService.Application.Repository.Interface;
using EventTicketBookingService.Domain.Models;

namespace EventTicketBookingService.Application.Services
{
    public class BookingService : IBookingService
    {
        private static readonly SemaphoreSlim _bookingLock = new(1, 1);
        private readonly IBookingRepository _bookingRepository;
        private readonly IEventRepository _eventRepository;
        public BookingService(IBookingRepository bookingRepository,
                              IEventRepository eventRepository)
        {
            _bookingRepository = bookingRepository;
            _eventRepository = eventRepository;
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
                        Id = await _bookingRepository.GetNextIdAsync(cancellationToken),
                        EventId = eventId,
                        Status = BookingStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        ProcessedAt = null,
                    };

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

        public async Task<BookingResponse>? GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken = default)
        {
            var existingBooking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);
            if (existingBooking == null)
                throw new ResourceNotFoundException($"Бронь с ID: {bookingId} не найдена");
            return ToInfo(existingBooking);
        }

        private static BookingResponse ToInfo(Booking booking) => new BookingResponse
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status,
            CreatedAt = booking.CreatedAt,
            ProcessedAt = booking.ProcessedAt,
        };
    }
}
