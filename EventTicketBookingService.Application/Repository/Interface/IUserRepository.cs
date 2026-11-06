using EventTicketBookingService.Domain.Models;

namespace EventTicketBookingService.Application.Repository.Interface
{
    public interface IUserRepository
    {
        public Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
        public Task<User?> GetUserByLoginAsync(string login, CancellationToken cancellationToken = default);
        public Task AddUserAsync(User user, CancellationToken cancellationToken = default);
        public Task<bool> ExistsByLoginAsync(string login, CancellationToken cancellationToken = default);
        public Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
