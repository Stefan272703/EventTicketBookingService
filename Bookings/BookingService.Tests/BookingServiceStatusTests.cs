using EventTicketBookingService.Domain.Models;

namespace BookingService.Tests
{
    public class BookingServiceStatusTests : BaseBookingTest
    {
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
            var userId = await CreateTestUserAsync();

            // Act - создаём бронь (занимаем последнее место)
            var response = await _bookingService.CreateBookingAsync(eventId, userId);
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
            var newResponse = await _bookingService.CreateBookingAsync(eventId, userId);
            Assert.NotNull(newResponse);
            Assert.Equal(0, @event.AvailableSeats); // снова занято
        }
    }
}
