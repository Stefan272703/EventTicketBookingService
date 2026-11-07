using EventTicketBookingService.Application.DTO;
using EventTicketBookingService.Application.Interfaces;
using EventTicketBookingService.Application.Repository.Interface;
using EventTicketBookingService.Domain.Exceptions;
using EventTicketBookingService.Domain.Models;
using Microsoft.Extensions.Logging;

namespace EventTicketBookingService.Application.Services
{
    public class BookingService : IBookingService
    {
        private static readonly SemaphoreSlim _bookingLock = new(1, 1);
        private readonly IBookingRepository _bookingRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IUserRepository _userRepository;

        public BookingService(IBookingRepository bookingRepository,
                              IEventRepository eventRepository,
                              IUserRepository userRepository)
        {
            _bookingRepository = bookingRepository;
            _eventRepository = eventRepository;
            _userRepository = userRepository;
        }

        public async Task<BookingResponse> CreateBookingAsync(int eventId, int userId, CancellationToken cancellationToken = default)
        {
            await _bookingLock.WaitAsync(cancellationToken);
            try
            {
                // Получаем пользователя по Id
                var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
                if (user == null)
                    throw new ResourceNotFoundException($"Пользователь с Id: {userId} не найден");

                // Получаем событие по Id
                var @event = await _eventRepository.GetEventByIdAsync(eventId, cancellationToken);
                if (@event == null)
                    throw new ResourceNotFoundException($"Не удалось создать бронь к несуществующему событию с ID: {eventId}");
                

                var now = DateTime.UtcNow;
                // Проверка, что бронь происходит не для прошедшего или уже идущего события
                @event.EnsureNotPassed(now);

                // Проверка на лимит активных броней
                var activeBookingsCount = await _bookingRepository.CountActiveByUserIdAsync(userId, cancellationToken);
                user.EnsureCanCreateBooking(activeBookingsCount);

                // Попытка зарезервировать место
                if (@event.TryReserveSeats())
                {
                    // Создание брони прямо сейчас
                    Booking booking = Booking.Create(eventId, userId, now);

                    await _bookingRepository.AddBookingAsync(booking, cancellationToken);
                    await _bookingRepository.SaveChangesAsync(cancellationToken);

                    BookingResponse response = new BookingResponse()
                    {
                        Id = booking.Id,
                        UserId = booking.UserId,
                        EventId = booking.EventId,
                        Status = booking.Status,
                        CreatedAt = booking.CreatedAt
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

        public async Task CancelBookingAsync(int bookingId, int actingUserId, CancellationToken cancellationToken = default)
        {
            // Получаем бронь по Id
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);
            if(booking == null)
                throw new ResourceNotFoundException($"Бронь с Id: {bookingId} не найдена");

            // Получаем пользователя по Id
            var actingUser = await _userRepository.GetUserByIdAsync(actingUserId, cancellationToken);
            if(actingUser == null)
                throw new ResourceNotFoundException($"Пользователь с Id: {actingUserId} не найден");
            // Получаем событие по Id 
            var @event = await _eventRepository.GetEventByIdAsync(booking.EventId, cancellationToken);
            if(@event == null)
                throw new ResourceNotFoundException($"Событие с Id: {booking.EventId} не найдено");

            // Проверка, что есть права на изменение состояния брони
            actingUser.EnsureCanCancelBooking(booking);
            // Отмена брони (перевод в статус Cancel)
            booking.Cancel();
            // Освобождаем место
            @event.ReleaseSeats();
            // Сохранение изменений в репозитории бронирования
            await _bookingRepository.SaveChangesAsync();
        }

        private static BookingResponse ToInfo(Booking booking) => new BookingResponse
        {
            Id = booking.Id,
            EventId = booking.EventId,
            UserId= booking.UserId,
            Status = booking.Status,
            CreatedAt = booking.CreatedAt,
            ProcessedAt = booking.ProcessedAt,
        };
    }
}
