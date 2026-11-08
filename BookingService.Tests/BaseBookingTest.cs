using EventTicketBookingService.Application.DTO;
using EventTicketBookingService.Application.Interfaces;
using EventTicketBookingService.Application.Repository.Interface;
using EventTicketBookingService.Domain.Models;
using EventTicketBookingService.Infrastructure.DataAccess;
using EventTicketBookingService.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Tests
{
    public class BaseBookingTest
    {
        protected readonly ServiceProvider _serviceProvider;
        protected readonly IServiceScope _scope;
        protected readonly IEventService _eventService;
        protected readonly IBookingService _bookingService;

        public BaseBookingTest()
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
            services.AddScoped<IEventService, EventTicketBookingService.Application.Services.EventService>();
            services.AddScoped<IBookingService, EventTicketBookingService.Application.Services.BookingService>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            _serviceProvider = services.BuildServiceProvider();
            _scope = _serviceProvider.CreateScope();
            _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
            _bookingService = _scope.ServiceProvider.GetRequiredService<IBookingService>();
        }

        public void Dispose()
        {
            _scope.Dispose();
            _serviceProvider.Dispose();
        }

        protected async Task<int> CreateTestEventAsync(int totalSeats = 10, DateTime? startAt = null)
        {
            var futureDate = startAt ?? DateTime.UtcNow.AddDays(1);
            var created = await _eventService.CreateEventAsync(new EventInfo
            {
                Title = "Test Event",
                StartAt = futureDate,
                EndAt = futureDate.AddHours(2),
                TotalSeats = totalSeats
            });
            return created.Id;
        }

        protected async Task<int> CreateTestUserAsync(string login = "testuser")
        {
            var context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = new User(login, "hashed_password_123");
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user.Id;
        }

        protected async Task<Event> GetEventAsync(int eventId)
        {
            var context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var @event = await context.Events.FirstAsync(e => e.Id == eventId);
            return @event;
        }
    }
}
