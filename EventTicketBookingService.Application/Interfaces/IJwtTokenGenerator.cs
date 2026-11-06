using EventTicketBookingService.Domain.Models;

namespace EventTicketBookingService.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        // Генерация токена по Id, логину и роли пользователя
        public string GenerateToken(int userId, string login , UserRoles role);
    }
}
