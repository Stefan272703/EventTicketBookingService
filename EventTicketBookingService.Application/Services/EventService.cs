using EventTicketBookingService.Domain.Exceptions;
using EventTicketBookingService.Application.Interfaces;
using EventTicketBookingService.Domain.Models;
using EventTicketBookingService.Application.Repository.Interface;
using EventTicketBookingService.Application.DTO;
using System.ComponentModel.DataAnnotations;

namespace EventTicketBookingService.Application.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        // Получить все события
        public async Task<PaginatedResultDTO<EventInfo>> GetAllEventsAsync(string title,
            DateTime? from,
            DateTime? to,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var (items, totalCount) = await _eventRepository.GetFilteredEvents(title, from, to, page, pageSize, cancellationToken);

            return new PaginatedResultDTO<EventInfo>()
            {
                TotalCount = totalCount,
                Events = items.Select(ToInfo).ToArray(),
                PageIndex = page,
                PageSizeByIndex = items.Count
            };
        }

        // Получить событие по Id
        public async Task<EventInfo?> GetEventByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var eventById = await _eventRepository.GetEventByIdAsync(id, cancellationToken);
            if (eventById == null)
                throw new ResourceNotFoundException($"Не найдено событие по ID: {id}");

            return ToInfo(eventById);
        }

        // Создать новое событие
        public async Task<EventInfo?>? CreateEventAsync(EventInfo createdEvent, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(createdEvent.Title))
                throw new ValidationException("Title не может быть пустым");
            if (createdEvent.StartAt >= createdEvent.EndAt)
                throw new ValidationException("Конец события должен быть позже начала события");

            var @event = new Event(createdEvent.TotalSeats.Value)
            {
                // ToDo: Исправить момент, когда вызываем несколько раз БД
                Id = await _eventRepository.GetNextIdAsync(cancellationToken),
                Title = createdEvent.Title,                         // Название события
                Description = createdEvent.Description,             // Описание события из тела запроса Event
                StartAt = createdEvent.StartAt,
                EndAt = createdEvent.EndAt,
            };

            // Добаляем событие
            await _eventRepository.AddEventAsync(@event, cancellationToken);
            // Обновляем данные в БД
            await _eventRepository.SaveChangesAsync(cancellationToken);
            return ToInfo(@event);
        }

        // Обновить событие целиком
        public async Task<EventInfo> UpdateEventAsync(int id, EventInfo createdEvent, CancellationToken cancellationToken = default)
        {
            var existingEvent = await _eventRepository.GetEventByIdAsync(id, cancellationToken);
            if (existingEvent == null)
                throw new ResourceNotFoundException($"Не найдено событие по ID: {id}");
            if (string.IsNullOrWhiteSpace(createdEvent.Title))
                throw new ValidationException("Title не может быть пустым");
            if (createdEvent.StartAt >= createdEvent.EndAt)
                throw new ValidationException("Конец события должен быть позже начала события");

            var updatedEvent = await _eventRepository.UpdateEventByIdAsync(id, createdEvent, cancellationToken);

            // Обновляем в БД значения
            await _eventRepository.SaveChangesAsync(cancellationToken);

            return ToInfo(existingEvent);
        }

        // Удалить событие
        public async Task<EventInfo> DeleteEventAsync(int id, CancellationToken cancellationToken = default)
        {
            var delEvent = await _eventRepository.RemoveEventByIdAsync(id, cancellationToken);
            if (delEvent == null)
                throw new ResourceNotFoundException($"Не найдено событие по ID: {id}");

            // Обновляем в БД значение
            await _eventRepository.SaveChangesAsync(cancellationToken);
            return ToInfo(delEvent);
        }


        private static EventInfo ToInfo(Event @event) => new EventInfo
        {
            Id = @event.Id,
            Title = @event.Title,                         // Название события
            Description = @event.Description,             // Описание события из тела запроса Event
            StartAt = @event.StartAt,
            EndAt = @event.EndAt,
            TotalSeats = @event.TotalSeats,
            AvailableSeats = @event.AvailableSeats
        };
    }
}
