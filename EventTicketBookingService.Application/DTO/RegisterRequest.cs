using EventTicketBookingService.Domain.Models;

namespace EventTicketBookingService.Application.DTO
{
    public class RegisterRequest
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserRoles Role { get; set; } = UserRoles.User; // По умолчанию User
    }
}
