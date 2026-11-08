using EventTicketBookingService.Application.Repository.Interface;
using EventTicketBookingService.Domain.Models;
using EventTicketBookingService.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventTicketBookingService.Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task AddUserAsync(User user, CancellationToken cancellationToken = default)
        {
            return _context.Users.AddAsync(user, cancellationToken).AsTask();
        }

        public Task<bool> ExistsByLoginAsync(string login, CancellationToken cancellationToken = default)
        {
            return _context.Users.AnyAsync(u => u.Login == login, cancellationToken);
        }

        public Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return _context.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public Task<User?> GetUserByLoginAsync(string login, CancellationToken cancellationToken = default)
        {
            return _context.Users.FirstOrDefaultAsync(x => x.Login == login, cancellationToken);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
