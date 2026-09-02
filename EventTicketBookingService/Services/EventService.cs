using EventTicketBookingService.Exceptions;
using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace EventTicketBookingService.Services
{
    public class EventService: IEventService
    {
        private List<Event> _events = [];

        // Получить все события
        public PaginatedResultDTO<Event> GetAllEvents(string title, 
            DateTime? from,
            DateTime? to,
            int page,
            int pageSize)
        {
            IEnumerable<Event> filteredEvents = _events;
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
            var paginatedEvents = GetEventsWithPagination(filteredEvents, page, pageSize);

            return paginatedEvents;
        }
            
        // Метод получения результата пагинации
        private PaginatedResultDTO<Event> GetEventsWithPagination(
            IEnumerable<Event> entryEvents,
            int page,
            int pageSize)
        {
            // пагинация фильтрованного списка событий
            var items = entryEvents.Skip((page - 1) * pageSize).Take(pageSize);

            // Общее количество событий
            int totalCount = entryEvents.Count();
            // Количество элементов на текущей странице
            int pageSizeByIndex = items.Count();

            PaginatedResultDTO<Event> paginatedResultDTO = new PaginatedResultDTO<Event> 
            { 
                TotalCount = totalCount,
                Events = items,
                PageIndex = page,
                PageSizeByIndex = pageSizeByIndex
            };

            return paginatedResultDTO;
        }

        // Получить событие по Id
        public Event? GetEventById(int id)
        {
            var eventById =  _events?.FirstOrDefault(x => x.Id == id);
            if (eventById == null)
                throw new ResourceNotFoundException(eventById, $"Не найдено событие по ID: {id}");

            return eventById;
        }

        // Создать новое событие
        public async Task<EventInfo?>? CreateEventAsync(EventInfo createdEvent)
        {
            if (string.IsNullOrWhiteSpace(createdEvent.Title))
                throw new ValidationException("Title не может быть пустым");
            if (createdEvent.StartAt >= createdEvent.EndAt)
                throw new ValidationException("Конец события должен быть позже начала события");

            var @event = new Event(createdEvent.TotalSeats.Value)
            {   
                Id = _events.Any() ? _events.Max(x => x.Id) + 1 : 1,
                Title = createdEvent.Title,                         // Название события
                Description = createdEvent.Description,             // Описание события из тела запроса Event
                StartAt = createdEvent.StartAt,
                EndAt = createdEvent.EndAt,
            };

            _events?.Add(@event);

            var eventInfo = new EventInfo()
            {
                Id = @event.Id,
                Title = @event.Title,                         // Название события
                Description = @event.Description,             // Описание события из тела запроса Event
                StartAt = @event.StartAt,
                EndAt = @event.EndAt,
                TotalSeats = @event.TotalSeats,
                AvailableSeats = @event.AvailableSeats
            };

            return eventInfo;
        }

        // Обновить событие целиком
        public Event UpdateEvent(int id, EventInfo createdEvent)
        {
            var existingEvent = _events.FirstOrDefault(x => x.Id == id);
            if (existingEvent == null)
                throw new ResourceNotFoundException(existingEvent, $"Не найдено событие по ID: {id}");
            if (string.IsNullOrWhiteSpace(createdEvent.Title))
                throw new ValidationException("Title не может быть пустым");
            if (createdEvent.StartAt >= createdEvent.EndAt)
                throw new ValidationException("Конец события должен быть позже начала события");

            existingEvent?.Title = createdEvent.Title;
            existingEvent?.Description = createdEvent.Description;
            existingEvent?.StartAt = createdEvent.StartAt;
            existingEvent?.EndAt = createdEvent.EndAt;

            return existingEvent;
        }

        // Удалить событие
        public Event DeleteEvent(int id)
        {
            var delEvent = _events.FirstOrDefault(x => x.Id == id);
            if (delEvent == null)
                throw new ResourceNotFoundException(delEvent, $"Не найдено событие по ID: {id}");
            _events.Remove(delEvent);
            return delEvent;
        }

    }
}
