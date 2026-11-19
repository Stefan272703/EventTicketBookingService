using BookingService.Application.DTO;
using BookingService.Application.Interfaces;
using BookingService.Application.Repository.Interface;
using BuildingBlocks.Domain.Exceptions;
using BookingService.Domain.Models;

namespace BookingService.Application.Services
{
    public class BookingService : IBookingService
    {
        private static readonly SemaphoreSlim _bookingLock = new(1, 1);
        private const int MaxActiveBookings = 10;
        private readonly IBookingRepository _bookingRepository;

        public BookingService(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<BookingResponse> CreateBookingAsync(int eventId, int userId, CancellationToken cancellationToken = default)
        {
            await _bookingLock.WaitAsync(cancellationToken);
            try
            {
                // Проверка на лимит активных броней
                var activeBookingsCount = await _bookingRepository.CountActiveByUserIdAsync(userId, cancellationToken);

                // Если количество активных броней превышает или равно максимальное количество броней у пользователя
                if (activeBookingsCount >= MaxActiveBookings)
                    throw new BookingLimitExceededException($"Пользователь с Id: {userId} превысил количество создания активных броней");

                // Время создания брони прямо сейчас
                var now = DateTime.UtcNow;
                // Создание брони прямо сейчас
                Booking booking = Booking.Create(eventId, userId, now);

                // Добавляем бронь в БД
                await _bookingRepository.AddBookingAsync(booking, cancellationToken);
                // Сохраняем изменения в БД
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

        public async Task CancelBookingAsync(int bookingId, int actingUserId, string actingUserRole, CancellationToken cancellationToken = default)
        {
            // Получаем бронь по Id
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);
            if (booking == null)
                throw new ResourceNotFoundException($"Бронь с Id: {bookingId} не найдена");

            var isAdmin = string.Equals(actingUserRole, "Admin", StringComparison.OrdinalIgnoreCase);
            if(!isAdmin && booking.UserId != actingUserId)
                throw new OperationNotPermittedException($"Пользователь {actingUserId} не имеет прав на отмену чужой брони {bookingId}");

            // Отмена брони (перевод в статус Cancel)
            booking.Cancel();
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
