using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Extensions.Hosting;
using Moq;

namespace BookingService.Tests
{
    public class BookingServiceTests
    {
        private readonly Mock<IBookingTaskQueue> _taskStoreMock;
        private readonly Mock<IEventService> _eventServiceMock;
        private readonly EventTicketBookingService.Services.BookingService _bookingService;

        public BookingServiceTests()
        {
            _taskStoreMock = new Mock<IBookingTaskQueue>();
            _eventServiceMock = new Mock<IEventService>();
            _bookingService = new EventTicketBookingService.Services.BookingService(_taskStoreMock.Object,
                                                                               _eventServiceMock.Object);
        }

        // Создание брони для существующего события — возвращается BookingInfo со статусом Pending;
        [Fact]
        public async Task CreateBookingAsync_ExistingEvent_ReturnsBookingInfoByPending()
        {
            // Arrange
            const int eventId = 1;
            var eventDto = new EventDTO { Id = eventId };
            _eventServiceMock.Setup(x =>x.GetEventById(eventId)).Returns(eventDto);

            // Act
            var result = await _bookingService.CreateBookingAsync(eventId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(BookingStatus.Pending, result.Status);
            _taskStoreMock.Verify(x => x.Enqueue(It.IsAny<Booking>()), Times.Once);
        }

        // Создание нескольких броней для одного события — все создаются с уникальными Id
        [Fact]
        public async Task CreateBookingAsync_MiltipleBookingsForSameEvent_GenerateUniqueIds()
        {
            // Arrange
            const int eventId = 1;
            var eventDto = new EventDTO { Id = eventId };
            _eventServiceMock.Setup(x => x.GetEventById(eventId)).Returns(eventDto);


            // Act
            var booking1 = await _bookingService.CreateBookingAsync(eventId);
            var booking2 = await _bookingService.CreateBookingAsync(eventId);
            var booking3 = await _bookingService.CreateBookingAsync(eventId);

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
            const int eventId = 1;
            var eventDto = new EventDTO { Id = eventId };
            _eventServiceMock.Setup(x => x.GetEventById(eventId)).Returns(eventDto);

            var created = await _bookingService.CreateBookingAsync(eventId);

            // Act
            var result = await _bookingService.GetBookingByIdAsync(created.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(result.Id, created.Id);
            Assert.Equal(result.EventId, eventId);
            Assert.Equal(result.Status, BookingStatus.Pending);
        }

        // Получение брони отражает изменение статуса (после Confirm/Reject).
        [Fact]
        public async Task GetBookingByIdAsync_AfterStatusChange_ReflectsUpdatedStatus()
        {
            // Arrange
            const int eventId = 1;
            var eventDto = new EventDTO { Id = eventId };
            _eventServiceMock.Setup(x => x.GetEventById(eventId)).Returns(eventDto);

            var created = await _bookingService.CreateBookingAsync(eventId);
            var bookingId = created.Id;

            // Act
            await _bookingService.UpdateBookingStatusAsync(bookingId, BookingStatus.Confirmed, CancellationToken.None);

            var updated = await _bookingService.GetBookingByIdAsync(bookingId);

            // Assert
            Assert.NotNull(updated);
            Assert.Equal(BookingStatus.Confirmed, updated.Status);
            Assert.NotNull(updated.ProcessedAt);
        }
    }
}
