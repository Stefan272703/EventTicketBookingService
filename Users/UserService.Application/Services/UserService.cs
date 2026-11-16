using System.ComponentModel.DataAnnotations;
using UserService.Application.DTO;
using UserService.Application.Interfaces;
using UserService.Application.Repository.Interface;
using UserService.Domain.Models;

namespace UserService.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public UserService(IUserRepository userRepository,
                           IPasswordHasher passwordHasher, 
                           IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            // Если существует пользователь с таим логином
            if (await _userRepository.ExistsByLoginAsync(request.Login, cancellationToken))
                throw new ValidationException("Пользователь с таким логином уже существует");

            // Хешируем пароль
            var passwordHash = _passwordHasher.Hash(request.Password);

            // Создаем пользователя
            var user = new User(request.Login, passwordHash, UserRoles.User);

            await _userRepository.AddUserAsync(user, cancellationToken);
            await _userRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetUserByLoginAsync(request.Login, cancellationToken);

            if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
                throw new ValidationException("Неверный логин или пароль");

            var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Login, user.Role);

            return new AuthResponse { Token = token };
        }
    }
}
