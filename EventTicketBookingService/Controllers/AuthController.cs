using EventTicketBookingService.Application.DTO;
using EventTicketBookingService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventTicketBookingService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            await _userService.RegisterAsync(request, cancellationToken);
            return Ok(new { Message = "Пользователь успешно зарегистрирован" });
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
        {
            var response = await _userService.LoginAsync(request, ct);
            return Ok(response);
        }
    }
}
