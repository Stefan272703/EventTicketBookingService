using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace EventTicketBookingService.Services
{
    public class EventService: IEventService
    {
        private static List<Event> _events = []; //[ new Event { Id = 1, Title="Сказка", Description = "LOL", StartAt = DateTime.Now, EndAt = DateTime.Now}];

        // Получить все события
        public List<Event> GetAllEvents()
        {
            return _events;
        }

        // Получить событие по Id
        public Event? GetEventById(int id)
        {
            return _events?.FirstOrDefault(x => x.Id == id);
        }

        // Создать новое событие
        public Event? CreateEvent([FromBody] Event my_event)
        {


            var new_event = new Event() 
            { 
                Id = _events.Any() ? _events.Count() + 1 : 1,
                Title = my_event.Title,                         // Название события
                Description = my_event.Description,             // Описание события из тела запроса Event
                StartAt = my_event.StartAt,
                EndAt = my_event.EndAt,

            };

            _events?.Add(new_event);
            return new_event;
        }

        // Обновить событие целиком
        public Event UpdateEvent(int id, [FromBody] Event my_event)
        {
            var existingEvent = _events.FirstOrDefault(x => x.Id == id);
            existingEvent?.Title = my_event.Title;
            existingEvent?.Description = my_event.Description;
            existingEvent?.StartAt = my_event.StartAt;
            existingEvent?.EndAt = my_event.EndAt;


            return existingEvent;
        }

        // Удалить событие
        public Event DeleteEvent(int id)
        {
            var delEvent = _events.FirstOrDefault(x => x.Id == id);
            _events.Remove(delEvent);
            return delEvent;
        }

    }
}
