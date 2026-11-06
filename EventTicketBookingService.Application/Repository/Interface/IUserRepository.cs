using EventTicketBookingService.Domain.Models;

namespace EventTicketBookingService.Application.Repository.Interface
{
    public interface IUserRepository
    {
        // Получение пользователя по его Id из репозитория пользователя
        public Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);

        // Получение пользователя по его логину из репозитория пользователя
        public Task<User?> GetUserByLoginAsync(string login, CancellationToken cancellationToken = default);

        // Добавит пользователя в репозиторий пользователя
        public Task AddUserAsync(User user, CancellationToken cancellationToken = default);

        // Проверка на наличие данного логина в репозитории пользователя
        public Task<bool> ExistsByLoginAsync(string login, CancellationToken cancellationToken = default);

        // Сохранение данных в БД, таблицу пользователя
        public Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
