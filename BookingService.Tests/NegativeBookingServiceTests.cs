using EventTicketBookingService.Exceptions;
using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookingService.Tests
{
    public class NegativeBookingServiceTests
    {
        private readonly Mock<IBookingTaskQueue> _taskStoreMock;
        private readonly Mock<IEventStore> _eventStoreMock;
        private readonly EventTicketBookingService.Services.BookingService _bookingService;

        public NegativeBookingServiceTests()
        {
            _taskStoreMock = new Mock<IBookingTaskQueue>();
            _eventStoreMock = new Mock<IEventStore>();
            _bookingService = new EventTicketBookingService.Services.BookingService(_taskStoreMock.Object,
                                                                                    _eventStoreMock.Object);
        }

        // Создание брони для несуществующего события;
        [Fact]
        public async Task CreateBookingAsync_ForNonExistentEvent_ThrowsResourceNotFoundException()
        {
            // Arrange
            const int eventId = 999;

            _eventStoreMock.Setup(x => x.TryGetEventById(eventId, out It.Ref<Event?>.IsAny))
               .Returns((int id, out Event? ev) =>
               {
                   ev = new Event(5) { Id = id };
                   return false;
               });

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(async () => await _bookingService.CreateBookingAsync(eventId));
            _taskStoreMock.Verify(x => x.Enqueue(It.IsAny<Booking>()), Times.Never);

        }

        // Создание брони для удаленного события;
        [Fact]
        public async Task CreateBookingAsync_ForDeletedEvent_ThrowsResourceNotFoundException()
        {
            // Arrange
            const int eventId = 1;

            _eventStoreMock.Setup(x => x.TryGetEventById(eventId, out It.Ref<Event?>.IsAny))
               .Returns((int id, out Event? ev) =>
               {
                   ev = new Event(5) { Id = id };
                   return false;
               });

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(async () => await _bookingService.CreateBookingAsync(eventId));
            _taskStoreMock.Verify(x => x.Enqueue(It.IsAny<Booking>()), Times.Never);

        }

        // Получение брони по несуществующему Id.
        [Fact]
        public async Task GetBookingByIdAsync_WithNonExistentId_ReturnsNull()
        {
            // Arrange
            const int invalidId = 999;

            _eventStoreMock.Setup(x => x.TryGetEventById(invalidId, out It.Ref<Event?>.IsAny))
               .Returns((int id, out Event? ev) =>
               {
                   ev = new Event(5) { Id = id };
                   return true;
               });

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(async () => await _bookingService.GetBookingByIdAsync(invalidId));

        }
    }
}
