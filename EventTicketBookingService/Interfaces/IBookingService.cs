using EventTicketBookingService.Models;

namespace EventTicketBookingService.Interfaces
{
    public interface IBookingService
    {
        // Создание брони для указанного события
        public Task<BookingResponse>? CreateBookingAsync(int eventId, CancellationToken cancellationToken = default);

        // Получение брони по идентификатору
        public Task<Booking>? GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken = default);

        // Обновление брони
        public Task UpdateBookingStatusAsync(int bookingId, BookingStatus status, CancellationToken cancellationToken = default);
    }
}
