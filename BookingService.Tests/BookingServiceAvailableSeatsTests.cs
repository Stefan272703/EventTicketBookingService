using EventTicketBookingService.DataAccess;
using EventTicketBookingService.Exceptions;
using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using EventTicketBookingService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookingService.Tests
{
    public class BookingServiceAvailableSeatsTests
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;

        public BookingServiceAvailableSeatsTests()
        {
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

        private async Task<Event> GetEventAsync(int eventId)
        {
            var context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var @event = await context.Events.FirstAsync(e => e.Id == eventId);
            return @event;
        }

        [Fact]
        public async Task CreateBookingAsync_ValidEvent_DecreasesAvailableSeatsByOne()
        {
            // Arrange
            var eventId = await CreateTestEventAsync();
            var @event = await GetEventAsync(eventId);

            // Act
            var result = await _bookingService.CreateBookingAsync(@event.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(BookingStatus.Pending, result.Status);
            Assert.Equal(9, @event.AvailableSeats);
        }

        [Fact]
        public async Task CreateBookingAsync_MultipleUntilLimit_AllSuccessWithUniqueIds()
        {
            // Arrange
            int totalSeats = 5;
            var eventId = await CreateTestEventAsync(totalSeats);
            var @event = await GetEventAsync(eventId);

            var results = new List<BookingResponse>();

            // Act
            for (int i = 0; i < totalSeats; i++)
            {
                var response = await _bookingService.CreateBookingAsync(@event.Id);
                results.Add(response);
            }

            Assert.Equal(totalSeats, results.Count);
            var ids = results.Select(r => r.Id).Distinct();
            Assert.Equal(totalSeats, ids.Count()); // Все Id уникальны
            Assert.Equal(0, @event.AvailableSeats); // все места заняты
        }

        [Fact]
        public async Task CreateBookingAsync_WhenNoSeatsLeft_ThrowsNoAvailableSeatsException()
        {
            // Arrange
            int totalSeats = 1;
            var eventId = await CreateTestEventAsync(totalSeats);// только 1 место
            var @event = await GetEventAsync(eventId);

            // Act
            await _bookingService.CreateBookingAsync(@event.Id);

            // Assert 
            await Assert.ThrowsAsync<NoAvailableSeatsException>(async () => await _bookingService.CreateBookingAsync(@eventId));
        }
    }
}
