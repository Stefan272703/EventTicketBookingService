using EventTicketBookingService.Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventTicketBookingService.Application.Interfaces
{
    public interface IUserService
    {
        // Регистрация нового пользователя
        public Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

        // Логин
        public Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    }
}
