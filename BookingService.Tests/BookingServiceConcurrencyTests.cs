using EventTicketBookingService.DataAccess;
using EventTicketBookingService.Exceptions;
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
    public class BookingServiceConcurrencyTests
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;

        public BookingServiceConcurrencyTests()
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

        [Fact]
        public async Task ConcurrentBooking_Overbooking_Exactly5SuccessAnd15Failures()
        {
            int totalSeats = 5;
            int requests = 20;

            var eventId = await CreateTestEventAsync(totalSeats);

            // Act - запускаем параллельные запросы
            var tasks = Enumerable.Range(0, requests)
                .Select(_ => Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    try
                    {
                        await bookingService.CreateBookingAsync(eventId);
                        return true;
                    }
                    catch (NoAvailableSeatsException)
                    {
                        return false;
                    }
                }
                ));

            var results = await Task.WhenAll(tasks);
            var successCount = results.Count(r => r);
            Assert.Equal(totalSeats, successCount);
        }

        [Fact]
        public async Task ConcurrentBooking_UniqueIdsGuaranteed()
        {
            // Arrange
            const int totalSeats = 10;
            const int requests = 10;
            var eventId = await CreateTestEventAsync(totalSeats);
            var bookingIds = new System.Collections.Concurrent.ConcurrentBag<int>();

            // Act - запускаем 10 параллельных запросов
            var tasks = Enumerable.Range(0, requests)
                .Select(_ => Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    var booking = await bookingService.CreateBookingAsync(eventId);
                    bookingIds.Add(booking.Id);
                }));

            await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(totalSeats, bookingIds.Distinct().Count()); // все уникальны
        }
    }
}
