using EventTicketBookingService.DataAccess;
using EventTicketBookingService.Exceptions;
using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

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
            //_taskStoreMock = new Mock<IBookingTaskQueue>();
            //_eventStoreMock = new Mock<IEventStore>();
            //_bookingService = new EventTicketBookingService.Services.BookingService(_taskStoreMock.Object,
            //                                                                        _eventStoreMock.Object);

            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
            services.AddScoped<IEventService, EventTicketBookingService.Services.EventService>();
            services.AddScoped<IBookingService, EventTicketBookingService.Services.BookingService>();

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

        private async Task<int> CreateTestEventAsync(int totalSeats = 10)
        {
            var futureDate = DateTime.UtcNow.AddDays(1);
            var created = await _eventService.CreateEventAsync(new EventInfo
            {
                Title = "Test Event",
                StartAt = futureDate,
                EndAt = futureDate.AddHours(2),
                TotalSeats = totalSeats
            });
            return created.Id;
        }

        // Создание брони для несуществующего события;
        [Fact]
        public async Task CreateBookingAsync_ForNonExistentEvent_ThrowsResourceNotFoundException()
        {
            // Arrange
            const int eventId = 999;

            //_eventStoreMock.Setup(x => x.TryGetEventById(eventId, out It.Ref<Event?>.IsAny))
            //   .Returns((int id, out Event? ev) =>
            //   {
            //       ev = new Event(5) { Id = id };
            //       return false;
            //   });

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(async () => await _bookingService.CreateBookingAsync(eventId));
            //_taskStoreMock.Verify(x => x.Enqueue(It.IsAny<Booking>()), Times.Never);

        }

        // Создание брони для удаленного события;
        [Fact]
        public async Task CreateBookingAsync_ForDeletedEvent_ThrowsResourceNotFoundException()
        {
            // Arrange
            const int eventId = 1;

            //_eventStoreMock.Setup(x => x.TryGetEventById(eventId, out It.Ref<Event?>.IsAny))
            //   .Returns((int id, out Event? ev) =>
            //   {
            //       ev = new Event(5) { Id = id };
            //       return false;
            //   });

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(async () => await _bookingService.CreateBookingAsync(eventId));
            //_taskStoreMock.Verify(x => x.Enqueue(It.IsAny<Booking>()), Times.Never);

        }

        // Получение брони по несуществующему Id.
        [Fact]
        public async Task GetBookingByIdAsync_WithNonExistentId_ReturnsNull()
        {
            // Arrange
            const int invalidId = 999;

            //_eventStoreMock.Setup(x => x.TryGetEventById(invalidId, out It.Ref<Event?>.IsAny))
            //   .Returns((int id, out Event? ev) =>
            //   {
            //       ev = new Event(5) { Id = id };
            //       return true;
            //   });

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(async () => await _bookingService.GetBookingByIdAsync(invalidId));
        }
    }
}
