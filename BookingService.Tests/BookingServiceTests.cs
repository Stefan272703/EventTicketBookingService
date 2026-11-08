using EventTicketBookingService.Domain.Models;

namespace BookingService.Tests
{
    public class BookingServiceTests : BaseBookingTest
    {
        // Создание брони для существующего события — возвращается BookingInfo со статусом Pending;
        [Fact]
        public async Task CreateBookingAsync_ExistingEvent_ReturnsBookingInfoByPending()
        {
            // Arrange
            var eventId = await CreateTestEventAsync();
            var userId = await CreateTestUserAsync();

            // Act
            var result = await _bookingService.CreateBookingAsync(eventId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(BookingStatus.Pending, result.Status);
        }

        // Создание нескольких броней для одного события — все создаются с уникальными Id
        [Fact]
        public async Task CreateBookingAsync_MiltipleBookingsForSameEvent_GenerateUniqueIds()
        {
            // Arrange
            var eventId = await CreateTestEventAsync();
            var userId = await CreateTestUserAsync();

            // Act
            var booking1 = await _bookingService.CreateBookingAsync(eventId, userId);
            var booking2 = await _bookingService.CreateBookingAsync(eventId, userId);
            var booking3 = await _bookingService.CreateBookingAsync(eventId, userId);

            // Assert
            Assert.NotEqual(booking1.Id, booking2.Id);
            Assert.NotEqual(booking1.Id, booking3.Id);
            Assert.NotEqual(booking2.Id, booking3.Id);
        }

        // Получение брони по Id — возвращается корректная информация;
        [Fact]
        public async Task GetBookingByIdAsync_WithValidId_ReturnsCorrectBooking()
        {
            // Arrange
            var eventId = await CreateTestEventAsync();
            var userId = await CreateTestUserAsync();

            var created = await _bookingService.CreateBookingAsync(eventId, userId);

            // Act
            var result = await _bookingService.GetBookingByIdAsync(created.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(result.Id, created.Id);
            Assert.Equal(eventId, result.EventId);
            Assert.Equal(BookingStatus.Pending, result.Status);
        }
    }
}
