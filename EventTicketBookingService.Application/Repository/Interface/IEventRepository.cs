using EventTicketBookingService.Application.DTO;
using EventTicketBookingService.Domain.Models;

namespace EventTicketBookingService.Application.Repository.Interface
{
    public interface IEventRepository
    {
        // Получение события по его Id из репозитория событий
        public Task<Event?> GetEventByIdAsync(int id, CancellationToken cancellationToken = default);

        // Добавление события в репозиторий событий
        public Task AddEventAsync(Event @event, CancellationToken cancellationToken = default);

        // Удалить событие из репозитория событий
        public Task<Event?> RemoveEventByIdAsync(int id, CancellationToken cancellationToken = default);

        // Получить Список событий в отфильрованнос состоянии из репозитория событий
        public Task<(IReadOnlyList<Event?> items, int totalCount)> GetFilteredEvents(string title,
            DateTime? from,
            DateTime? to,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        // Сохранение данных в БД, таблицу событий
        public Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
