using BookingService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingService.Presentation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize] // Весть контроллер требует токен
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("{id}", Name = nameof(GetBookingById))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBookingById(int id)
        {
            var bookingById = await _bookingService.GetBookingByIdAsync(id);

            return Ok(bookingById);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelBooking(int id)
        {
            // Извлекаем ID пользователя из токена
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized();

            await _bookingService.CancelBookingAsync(id, userId);

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
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var booking = await _bookingService.CreateBookingAsync(id, userId);

            return AcceptedAtRoute(nameof(GetBookingById), new { id = booking?.Id }, booking);
        }
    }
}
