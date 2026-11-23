using EventService.Application.DTO;

namespace EventService.Application.Interfaces
{
    public interface IEventService
    {
        // Получить список всех событий с фильтрацией
        public Task<PaginatedResultDTO<EventInfo>> GetAllEventsAsync(string title,
            DateTime? from,
            DateTime? to,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);
        // Получить событие по его Id
        public Task<EventInfo?> GetEventByIdAsync(int id, CancellationToken cancellationToken = default);
        // Создать событие по его телу запроса
        public Task<EventInfo?>? CreateEventAsync(EventInfo createdEvent, CancellationToken cancellationToken = default);
        // Обновить событие по его Id и телу запроса
        public Task<EventInfo> UpdateEventAsync(int id, EventInfo createdEvent, CancellationToken cancellationToken = default);
        // Удалить событие по его Id
        public Task<EventInfo> DeleteEventAsync(int id, CancellationToken cancellationToken = default);
        // Получить топ-10 событий
        public Task<IReadOnlyList<EventInfo>> GetTop10EventsAsync(CancellationToken cancellationToken);
    }
}
