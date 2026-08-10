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
        public IActionResult GetAll([FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string title = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
            )
        {
            var events = _eventService.GetAllEvents(title, from, to, page, pageSize);
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
        public IActionResult Create([FromBody] Event createdEvent)
        {
            if (!TryValidateModel(createdEvent))
            {
                return BadRequest(ModelState);
            }

            var eventDTO = _eventService.CreateEvent(createdEvent);
            return CreatedAtAction(nameof(GetById), new {id = eventDTO?.Id}, eventDTO);
        }
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Event createdEvent)
        {
            if (!TryValidateModel(createdEvent))
            {
                return BadRequest(ModelState);
            }

            var existingEvent = _eventService.UpdateEvent(id, createdEvent);
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
