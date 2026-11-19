using BuildingBlocks.Domain.Exceptions;
using EventService.Application.Interfaces;
using EventService.Application.Repository.Interface;
using EventService.Domain.Models;
using EventService.Infrastructure.DataAccess;
using EventService.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventService.Tests
{
    public class EventDomainTests
    {
        private readonly IEventService _eventService;
        private readonly ServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;

        public EventDomainTests()
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IEventService, EventService.Application.Services.EventService>();

            _serviceProvider = services.BuildServiceProvider();
            _scope = _serviceProvider.CreateScope();
            _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
        }

        public void Dispose()
        {
            _scope.Dispose();
            _serviceProvider.Dispose();
        }

        [Fact]
        public void TryReserveSeats_WhenNotEnoughSeats_ReturnsFalseAndDoesNotChange()
        {
            // Arrange
            var @event = new Event(totalSeats: 1);

            // Act
            var result = @event.TryReserveSeats(2);

            // Assert
            Assert.False(result);
            Assert.Equal(1, @event.AvailableSeats);   // не ушло в минус!
        }

        [Fact]
        public void EnsureNotPassed_WhenStarted_ThrowsPastEventBookingException()
        {
            // Arrange 
            var @event = new Event(totalSeats: 10);

            // Act
            @event.Update("Test", "desc",
                DateTime.UtcNow.AddHours(-2),
                DateTime.UtcNow.AddHours(-1));

            // Assert
            Assert.Throws<PastEventBookingException>(
                () => @event.EnsureNotPassed(DateTime.UtcNow));
        }
    }
}
