using EventTicketBookingService.Exceptions;
using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketBookingService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventsController : ControllerBase
    {

        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;

        public EventsController(IEventService eventService,
                                IBookingService bookingService)
        {
            _eventService = eventService;
            _bookingService = bookingService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string title = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
            )
        {
            var events = await _eventService.GetAllEventsAsync(title, from, to, page, pageSize);
            return Ok(events);
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetById(int id)
        {
            var eventbyId = await _eventService.GetEventByIdAsync(id);
            if (eventbyId == null)
            {
                return NotFound($"Не найдено событие по ID: {id}");
            }
            return Ok(eventbyId);

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] EventInfo createdEvent)
        {
            if (!TryValidateModel(createdEvent))
            {
                return BadRequest(ModelState);
            }

            var eventDTO = await _eventService.CreateEventAsync(createdEvent);
            return CreatedAtAction(nameof(GetById), new { id = eventDTO?.Id }, eventDTO);
        }
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] EventInfo createdEvent)
        {
            if (!TryValidateModel(createdEvent))
            {
                return BadRequest(ModelState);
            }

            var existingEvent = await _eventService.UpdateEventAsync(id, createdEvent);
            if (existingEvent == null)
            {
                return NotFound($"Данного события не существует по id {id}");
            }
            return Ok(existingEvent);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(int id)
        {
            var delEvent = await _eventService.DeleteEventAsync(id);
            if (delEvent == null)
            {
                return NotFound($"Данного события не существует по id {id}");
            }
            return NoContent();
        }

        [HttpPost("{id}/book")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateBooking(int id)
        {
            var booking = await _bookingService.CreateBookingAsync(id);

            return AcceptedAtRoute(nameof(BookingsController.GetBookingById), new { id = booking?.Id }, booking);
        }
    }
}
