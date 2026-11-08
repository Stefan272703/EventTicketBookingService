using EventTicketBookingService.Domain.Exceptions;
using EventTicketBookingService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Tests
{
    public class BookingServiceConcurrencyTests : BaseBookingTest
    {
        [Fact]
        public async Task ConcurrentBooking_Overbooking_Exactly5SuccessAnd15Failures()
        {
            int totalSeats = 5;
            int requests = 20;

            var eventId = await CreateTestEventAsync(totalSeats);
            var userId = await CreateTestUserAsync();

            // Act - запускаем параллельные запросы
            var tasks = Enumerable.Range(0, requests)
                .Select(_ => Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();

                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    try
                    {
                        await bookingService.CreateBookingAsync(eventId, userId);
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
            var userId = await CreateTestUserAsync();
            var bookingIds = new System.Collections.Concurrent.ConcurrentBag<int>();

            // Act - запускаем 10 параллельных запросов
            var tasks = Enumerable.Range(0, requests)
                .Select(_ => Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    var booking = await bookingService.CreateBookingAsync(eventId, userId);
                    bookingIds.Add(booking.Id);
                }));

            await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(totalSeats, bookingIds.Distinct().Count()); // все уникальны
        }
    }
}
