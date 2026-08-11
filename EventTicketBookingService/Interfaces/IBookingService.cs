using EventTicketBookingService.Models;

namespace EventTicketBookingService.Interfaces
{
    public interface IBookingService
    {
        // Создание брони для указанного события
        public Task<BookingResponse>? CreateBookingAsync(int eventId);

        // Получение брони по идентификатору
        public Task<Booking>? GetBookingByIdAsync(int bookingId);

    }
}
