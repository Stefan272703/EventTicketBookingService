using EventTicketBookingService.Infrastructure.DataAccess;
using EventTicketBookingService.Domain.Exceptions;
using EventTicketBookingService.Application.Interfaces;
using EventTicketBookingService.Infrastructure.Repository;
using EventTicketBookingService.Application.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Tests
{
    public class NegativeBookingServiceTests
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;

        public NegativeBookingServiceTests()
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
            services.AddScoped<IEventService, EventTicketBookingService.Application.Services.EventService>();
            services.AddScoped<IBookingService, EventTicketBookingService.Application.Services.BookingService>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();

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

        // Создание брони для несуществующего события;
        [Fact]
        public async Task CreateBookingAsync_ForNonExistentEvent_ThrowsResourceNotFoundException()
        {
            // Arrange
            const int eventId = 999;

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(async () => await _bookingService.CreateBookingAsync(eventId));

        }

        // Создание брони для удаленного события;
        [Fact]
        public async Task CreateBookingAsync_ForDeletedEvent_ThrowsResourceNotFoundException()
        {
            // Arrange
            const int eventId = 1;

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(async () => await _bookingService.CreateBookingAsync(eventId));
        }

        // Получение брони по несуществующему Id.
        [Fact]
        public async Task GetBookingByIdAsync_WithNonExistentId_ReturnsNull()
        {
            // Arrange
            const int invalidId = 999;

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(async () => await _bookingService.GetBookingByIdAsync(invalidId));
        }
    }
}
