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
        public EventDTO UpdateEvent(int id, Event my_event)
        {
            var existingEvent = _events.FirstOrDefault(x => x.Id == id);
            existingEvent?.Title = my_event.Title;
            existingEvent?.Description = my_event.Description;
            existingEvent?.StartAt = my_event.StartAt;
            existingEvent?.EndAt = my_event.EndAt;

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
