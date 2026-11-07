using EventTicketBookingService.Application.Interfaces;
using EventTicketBookingService.Application.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
            return Ok(eventbyId);

        }

        [HttpPost]
        [Authorize(Roles = "Admin")] // Только для Админа
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] EventInfo createdEvent)
        {
            var eventDTO = await _eventService.CreateEventAsync(createdEvent);
            return CreatedAtAction(nameof(GetById), new { id = eventDTO?.Id }, eventDTO);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Только для Админа
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, [FromBody] EventInfo createdEvent)
        {
            var existingEvent = await _eventService.UpdateEventAsync(id, createdEvent);

            return Ok(existingEvent);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Только для Админа
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(int id)
        {
            var delEvent = await _eventService.DeleteEventAsync(id);

            return NoContent();
        }

        [HttpPost("{id}/book")]
        [Authorize] // Трбуется любая авторизаия
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateBooking(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(!int.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var booking = await _bookingService.CreateBookingAsync(id, userId);

            return AcceptedAtRoute(nameof(BookingsController.GetBookingById), new { id = booking?.Id }, booking);
        }
    }
}
