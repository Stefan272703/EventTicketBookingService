using BookingService.Domain.Models;
using BuildingBlocks.Domain;

namespace BookingService.Tests
{
    public class BookingServiceStatusTests : BaseBookingTest
    {
        [Fact]
        public void Confirm_ChangesStatusToConfirmedAndSetsProcessedAt()
        {
            // Arrange
            var booking = Booking.Create(eventId: 1, userId: 1, DateTime.UtcNow);
            // Act
            booking.Confirm();

            // Assert
            Assert.Equal(BookingStatus.Confirmed, booking.Status);
            Assert.NotNull(booking.ProcessedAt);
            Assert.InRange(booking.ProcessedAt.Value, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        }

        [Fact]
        public void Reject_ChangesStatusToConfirmeAndSetsProcessedAt()
        {
            // Arrange
            var booking = Booking.Create(eventId: 1, userId: 1, DateTime.UtcNow);


            // Act
            booking.Reject();

            // Assert
            Assert.Equal(BookingStatus.Rejected, booking.Status);
            Assert.NotNull(booking.ProcessedAt);
            Assert.InRange(booking.ProcessedAt.Value, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        }
    }
}
