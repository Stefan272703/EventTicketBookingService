using BuildingBlocks.Domain.Exceptions;
using BookingService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Tests
{
    public class BookingServiceConcurrencyTests : BaseBookingTest
    {
        [Fact]
        public async Task ConcurrentBooking_UniqueIdsGuaranteed()
        {
            // Arrange
            const int totalSeats = 10;
            const int requests = 10;
            var bookingIds = new System.Collections.Concurrent.ConcurrentBag<int>();

            // Act - запускаем 10 параллельных запросов
            var tasks = Enumerable.Range(0, requests)
                .Select(_ => Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    var booking = await bookingService.CreateBookingAsync(eventId:1, userId:1);
                    bookingIds.Add(booking.Id);
                }));

            await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(totalSeats, bookingIds.Distinct().Count()); // все уникальны
        }
    }
}
