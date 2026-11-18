using EventTicketBookingService.Domain.Exceptions;

namespace BookingService.Tests
{
    public class NegativeBookingServiceTests : BaseBookingTest
    {
        // Создание брони для несуществующего события;
        [Fact]
        public async Task CreateBookingAsync_ForNonExistentEvent_ThrowsResourceNotFoundException()
        {
            // Arrange
            const int eventId = 999;
            var userId = await CreateTestUserAsync();

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(async () => await _bookingService.CreateBookingAsync(eventId, userId));

        }

        // Создание брони для удаленного события;
        [Fact]
        public async Task CreateBookingAsync_ForDeletedEvent_ThrowsResourceNotFoundException()
        {
            // Arrange
            const int eventId = 1;
            var userId = await CreateTestUserAsync();

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(async () => await _bookingService.CreateBookingAsync(eventId, userId));
        }

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
