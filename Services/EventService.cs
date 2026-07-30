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
        public List<EventDTO> GetAllEvents(string title, DateTime? from, DateTime? to)
        {
            // Проверка, что to передано по умолчанию, как минимальное значение
            // Если да, то присваиваем максимальное значение DateTime как по умолчанию
            if (to == DateTime.MinValue)
            {
                to = DateTime.MaxValue;
            }
                return _events.Where(t => t.Title.ToLower().Contains(title.ToLower()) &&
                                     (t.StartAt.CompareTo(from) >= 0) && // Начало события не раньше указанного
                                     t.EndAt.CompareTo(to) <= 0) // Конец события не позже указанного
                                     .ToList();
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
