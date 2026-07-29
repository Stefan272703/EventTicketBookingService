using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace EventTicketBookingService.Services
{
    public class EventService: IEventService
    {
        private static List<EventDTO> _events = [];

        // Получить все события
        public List<EventDTO> GetAllEvents()
        {
            return _events;
        }

        // Получить событие по Id
        public EventDTO? GetEventById(int id)
        {
            return _events?.FirstOrDefault(x => x.Id == id);
        }

        // Создать новое событие
        public EventDTO? CreateEvent(Event createdEvent)
        {
            var eventDTO = new EventDTO() 
            { 
                Id = _events.Any() ? _events.Max(x => x.Id) + 1 : 1,
                Title = createdEvent.Title,                         // Название события
                Description = createdEvent.Description,             // Описание события из тела запроса Event
                StartAt = createdEvent.StartAt,
                EndAt = createdEvent.EndAt,
            };

            _events?.Add(eventDTO);
            return eventDTO;
        }

        // Обновить событие целиком
        public EventDTO UpdateEvent(int id, Event createdEvent)
        {
            var existingEvent = _events.FirstOrDefault(x => x.Id == id);
            existingEvent?.Title = createdEvent.Title;
            existingEvent?.Description = createdEvent.Description;
            existingEvent?.StartAt = createdEvent.StartAt;
            existingEvent?.EndAt = createdEvent.EndAt;

            return existingEvent;
        }

        // Удалить событие
        public EventDTO DeleteEvent(int id)
        {
            var delEvent = _events.FirstOrDefault(x => x.Id == id);
            _events.Remove(delEvent);
            return delEvent;
        }

    }
}
