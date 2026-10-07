using EventTicketBookingService.DataAccess;
using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookingService.Tests
{
    public class BookingServiceStatusTests
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;

        public BookingServiceStatusTests()
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
        public void Confirm_ChangesStatusToConfirmedAndSetsProcessedAt()
        {
            // Arrange
            var booking = new Booking
            {
                Id = 1,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now,
                ProcessedAt = null
            };

            // Act
            booking.Confirm();

            // Assert
            Assert.Equal(BookingStatus.Confirmed, booking.Status);
            Assert.NotNull(booking.ProcessedAt);
            Assert.InRange(booking.ProcessedAt.Value, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        }

        [Fact]
        public void Reject_ChangesStatusToRejectedAndSetsProcessedAt()
        {
            // Arrange
            var booking = new Booking
            {
                Id = 1,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now,
                ProcessedAt = null
            };

            // Act
            booking.Reject();

            // Assert
            Assert.Equal(BookingStatus.Rejected, booking.Status);
            Assert.NotNull(booking.ProcessedAt);
            Assert.InRange(booking.ProcessedAt.Value, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        }

        [Fact]
        public async Task Reject_ReleasesSeatsAndAllowsNewBooking()
        {
            // Arrange
            int eventId = await CreateTestEventAsync(1);

            // Act - создаём бронь (занимаем последнее место)
            var response = await _bookingService.CreateBookingAsync(eventId);
            var @event = await GetEventAsync(eventId);

            Assert.Equal(0, /*eventEntity*/@event.AvailableSeats);

            var booking = new Booking
            {
                Id = response.Id,
                EventId = eventId,
                Status = BookingStatus.Pending
            };
            booking.Reject();
            @event.ReleaseSeats();

            // После освобождения должно стать 1 свободное место
            Assert.Equal(1, @event.AvailableSeats);

            // Теперь можем создать новую бронь
            var newResponse = await _bookingService.CreateBookingAsync(eventId);
            Assert.NotNull(newResponse);
            Assert.Equal(0, @event.AvailableSeats); // снова занято
        }
    }
}
