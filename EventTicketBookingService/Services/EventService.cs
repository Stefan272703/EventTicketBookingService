using EventTicketBookingService.DataAccess;
using EventTicketBookingService.Exceptions;
using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace EventTicketBookingService.Services
{
    public class EventService : IEventService
    {
        //private List<Event> _events = [];
        //private readonly IEventStore _eventStore;
        private readonly AppDbContext _context;

        public EventService(AppDbContext appDbContext)//IEventStore eventStore)
        {
            _context = appDbContext;
            //_eventStore = eventStore;
        }

        // Получить все события
        public async Task<PaginatedResultDTO<EventInfo>> GetAllEventsAsync(string title,
            DateTime? from,
            DateTime? to,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var filteredEvents = _context.Events.AsQueryable();
            if (!string.IsNullOrEmpty(title))
            {
                filteredEvents = filteredEvents.Where(t => t.Title.ToLower().Contains(title.ToLower()));
            }
            if (from.HasValue)
            {
                filteredEvents = filteredEvents.Where(t => t.StartAt >= from.Value);
            }
            if (to.HasValue)
            {
                filteredEvents = filteredEvents.Where(t => t.EndAt <= to.Value);
            }

            // Пагинация событий с результатом
            var paginatedEvents = await GetEventsWithPagination(filteredEvents, page, pageSize, cancellationToken);

            return paginatedEvents;
        }

        // Метод получения результата пагинации
        private async Task<PaginatedResultDTO<EventInfo>> GetEventsWithPagination(
            IQueryable<Event> entryEvents,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            // Общее количество событий
            int totalCount = await entryEvents.CountAsync(cancellationToken);
            // пагинация фильтрованного списка событий
            var items = await entryEvents.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            // Количество элементов на текущей странице
            int pageSizeByIndex = items.Count();

            var paginatedResultDTO = new PaginatedResultDTO<EventInfo>
            {
                TotalCount = totalCount,
                Events = items.Select(ToInfo).ToArray(),
                PageIndex = page,
                PageSizeByIndex = pageSizeByIndex
            };

            return paginatedResultDTO;
        }

        // Получить событие по Id
        public async Task<EventInfo?> GetEventByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var eventById = await _context.Events.FirstOrDefaultAsync(x => x.Id == id, cancellationToken); //_events?.FirstOrDefault(x => x.Id == id);
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
                Id = await _context.Events.AnyAsync(cancellationToken) ? _context.Events.Max(x => x.Id) + 1 : 1,
                Title = createdEvent.Title,                         // Название события
                Description = createdEvent.Description,             // Описание события из тела запроса Event
                StartAt = createdEvent.StartAt,
                EndAt = createdEvent.EndAt,
            };

            // Добаляем событие
            await _context.Events.AddAsync(@event, cancellationToken);
            // Обновляем данные в БД
            await _context.SaveChangesAsync(cancellationToken);
            //_events?.Add(@event);
            //_eventStore?.AddEvent(@event);
            return ToInfo(@event);
            //var eventInfo = new EventInfo()
            //{
            //    Id = @event.Id,
            //    Title = @event.Title,                         // Название события
            //    Description = @event.Description,             // Описание события из тела запроса Event
            //    StartAt = @event.StartAt,
            //    EndAt = @event.EndAt,
            //    TotalSeats = @event.TotalSeats,
            //    AvailableSeats = @event.AvailableSeats
            //};

            //return eventInfo;
        }

        // Обновить событие целиком
        public async Task<Event> UpdateEventAsync(int id, EventInfo createdEvent, CancellationToken cancellationToken = default)
        {
            var existingEvent = await _context.Events.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (existingEvent == null)
                throw new ResourceNotFoundException($"Не найдено событие по ID: {id}");
            if (string.IsNullOrWhiteSpace(createdEvent.Title))
                throw new ValidationException("Title не может быть пустым");
            if (createdEvent.StartAt >= createdEvent.EndAt)
                throw new ValidationException("Конец события должен быть позже начала события");

            existingEvent?.Title = createdEvent.Title;
            existingEvent?.Description = createdEvent.Description;
            existingEvent?.StartAt = createdEvent.StartAt;
            existingEvent?.EndAt = createdEvent.EndAt;

            // Обновляем в БД значения
            await _context.SaveChangesAsync(cancellationToken);

            return existingEvent;
        }

        // Удалить событие
        public async Task<Event> DeleteEventAsync(int id, CancellationToken cancellationToken = default)
        {
            var delEvent = await _context.Events.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (delEvent == null)
                throw new ResourceNotFoundException(delEvent, $"Не найдено событие по ID: {id}");
            
            // Удаляем событие
            _context.Events.Remove(delEvent);
            // Обновляем в БД значение
            await _context.SaveChangesAsync(cancellationToken);
            //_eventStore.RemoveEvent(delEvent);
            return delEvent;
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
