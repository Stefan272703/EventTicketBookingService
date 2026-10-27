using EventTicketBookingService.Application.DTO;
using EventTicketBookingService.Domain.Models;

namespace EventTicketBookingService.Application.Interfaces
{
    public interface IBookingService
    {
        // Создание брони для указанного события
        public Task<BookingResponse>? CreateBookingAsync(int eventId, CancellationToken cancellationToken = default);

        // Получение брони по идентификатору
        public Task<Booking>? GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken = default);
    }
}
