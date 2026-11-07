using EventTicketBookingService.Domain.Models;

namespace EventTicketBookingService.Application.Repository.Interface
{
    public interface IBookingRepository
    {
        // Получение брони по ее Id из репозитория бронирования
        public Task<Booking?> GetBookingByIdAsync(int id, CancellationToken cancellationToken = default);

        // Подсчет активных(обрабатывающихся или принятых) броней конкретного пользователя по его Id в репозитории бронирования
        public Task<int> CountActiveByUserIdAsync(int userId, CancellationToken cancellationToken = default);

        // Получение Id брони,который обрабатывается из репозитория бронирования
        public Task<List<int>> GetBookingPendingIdsAsync(CancellationToken cancellationToken);

        // Добавление брони в репозиторий бронирования 
        public Task AddBookingAsync(Booking booking, CancellationToken cancellationToken = default);

        // Сохранение данных в БД, таблицу бронирования
        public Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
