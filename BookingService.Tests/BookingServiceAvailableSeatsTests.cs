using EventTicketBookingService.Domain.Exceptions;
using EventTicketBookingService.Domain.Models;
using EventTicketBookingService.Application.DTO;
namespace BookingService.Tests
{
    public class BookingServiceAvailableSeatsTests : BaseBookingTest
    {
        [Fact]
        public async Task CreateBookingAsync_ValidEvent_DecreasesAvailableSeatsByOne()
        {
            // Arrange
            var eventId = await CreateTestEventAsync();
            var @event = await GetEventAsync(eventId);
            var userId = await CreateTestUserAsync();

            // Act
            var result = await _bookingService.CreateBookingAsync(@event.Id, userId);

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
            var userId = await CreateTestUserAsync();

            var results = new List<BookingResponse>();

            // Act
            for (int i = 0; i < totalSeats; i++)
            {
                var response = await _bookingService.CreateBookingAsync(@event.Id, userId);
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
            var userId = await CreateTestUserAsync();

            // Act
            await _bookingService.CreateBookingAsync(@event.Id, userId);

            // Assert 
            await Assert.ThrowsAsync<NoAvailableSeatsException>(async () => await _bookingService.CreateBookingAsync(@eventId, userId));
        }
    }
}
