using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketBookingService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("{id}", Name = nameof(GetBookingById))]
        public async Task<IActionResult> GetBookingById(int id)
        {
            var bookingById = await _bookingService.GetBookingByIdAsync(id);

            if(bookingById == null)
            {
                return NotFound($"Не найдена бронь по Id: {id}");
            }

            return Ok(bookingById);
        }
    }
}
