using EventTicketBookingService.Application.DTO;
using EventTicketBookingService.Domain.Models;

namespace EventTicketBookingService.Application.Interfaces
{
    public interface IBookingService
    {
        // Создание брони для указанного события
        public Task<BookingResponse>? CreateBookingAsync(int eventId, int userId, CancellationToken cancellationToken = default);

        // Получение брони по идентификатору
        public Task<BookingResponse>? GetBookingByIdAsync(int bookingId, CancellationToken cancellationToken = default);

        // Отмена брони по идентификатору брони и пользователем
        public Task CancelBookingAsync(int bookingId, int actingUserId, CancellationToken cancellationToken = default);
    }
}
