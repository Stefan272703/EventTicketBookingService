using BuildingBlocks.Domain.Exceptions;

namespace BookingService.Tests
{
    public class NegativeBookingServiceTests : BaseBookingTest
    {
        // Получение брони по несуществующему Id.
        [Fact]
        public async Task GetBookingByIdAsync_WithNonExistentId_ReturnsNull()
        {
            // Arrange
            const int invalidId = 999;

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(async () => await _bookingService.GetBookingByIdAsync(invalidId));
        }
    }
}
