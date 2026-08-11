using EventTicketBookingService.Models;

namespace EventTicketBookingService.Interfaces
{
    public interface IBookingService
    {
        // Создание брони для указанного события
        public BookingResponse? CreateBookingAsync(int eventId);

        // Получение брони по идентификатору
        public Booking? GetBookingByIdAsync(int bookingId);
    }
}
