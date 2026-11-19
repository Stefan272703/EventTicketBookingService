using UserService.Application.DTO;

namespace UserService.Application.Interfaces
{
    public interface IUserService
    {
        // Регистрация нового пользователя
        public Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

        // Логин
        public Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    }
}
