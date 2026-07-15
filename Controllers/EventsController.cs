using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketBookingService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventsController: ControllerBase
    {

        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var events = _eventService.GetAllEvents();
            return Ok(events);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var eventbyId = _eventService.GetEventById(id);
            if(eventbyId == null)
            {
                return NotFound($"Не найдено событие по ID: {id}");
            }
            return Ok(eventbyId);

        }

        [HttpPost]
        public IActionResult Create([FromBody] Event my_event)
        {
            if (!TryValidateModel(my_event))
            {
                return BadRequest(ModelState);
            }

            var new_event = _eventService.CreateEvent(my_event);
            return CreatedAtAction(nameof(GetById), new {id = new_event?.Id}, new_event);
        }
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Event my_event)
        {
            if (!TryValidateModel(my_event))
            {
                return BadRequest(ModelState);
            }

            var existingEvent = _eventService.UpdateEvent(id, my_event);
            if (existingEvent == null)
            {
                return NotFound($"Данного события не существует по id {id}");
            }
            return Ok(existingEvent);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var delEvent = _eventService.DeleteEvent(id);
            if (delEvent == null)
            {
                return NotFound($"Данного события не существует по id {id}");
            }
            return NoContent();
        }

    }
}
