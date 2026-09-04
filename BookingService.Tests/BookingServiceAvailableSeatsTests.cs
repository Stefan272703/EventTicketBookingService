using EventTicketBookingService.Exceptions;
using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookingService.Tests
{
    public class BookingServiceAvailableSeatsTests
    {
        private readonly Mock<IBookingTaskQueue> _taskStoreMock;
        private readonly Mock<IEventStore> _eventStoreMock;
        private readonly EventTicketBookingService.Services.BookingService _bookingService;

        public BookingServiceAvailableSeatsTests()
        {
            _taskStoreMock = new Mock<IBookingTaskQueue>();
            _eventStoreMock = new Mock<IEventStore>();
            _bookingService = new EventTicketBookingService.Services.BookingService(_taskStoreMock.Object,
                                                                                    _eventStoreMock.Object);
        }

        [Fact]
        public async Task CreateBookingAsync_ValidEvent_DecreasesAvailableSeatsByOne()
        {
            // Arrange
            int eventId = 1;
            var eventEntity = new Event(10) { Id = eventId };


            _eventStoreMock
                .Setup(x => x.TryGetEventById(eventId, out It.Ref<Event?>.IsAny))
                .Returns((int id, out Event? ev) =>
                {
                    ev = eventEntity;
                    return true;
                });

            // Act
            var result = await _bookingService.CreateBookingAsync(eventId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(BookingStatus.Pending, result.Status);
            Assert.Equal(9, eventEntity.AvailableSeats);
            _taskStoreMock.Verify(x => x.Enqueue(It.IsAny<Booking>()), Times.Once);
        }

        [Fact]
        public async Task CreateBookingAsync_MultipleUntilLimit_AllSuccessWithUniqueIds()
        {
            // Arrange
            int eventId = 1;
            int totalSeats = 5;
            var eventEntity = new Event(totalSeats) { Id = eventId };

            _eventStoreMock
                .Setup(x => x.TryGetEventById(eventId, out It.Ref<Event?>.IsAny))
                .Returns((int id, out Event? ev) =>
                {
                    ev = eventEntity;
                    return true;
                });

            var results = new List<BookingResponse>();

            // Act
            for(int i = 0; i < totalSeats; i++)
            {
                var response = await _bookingService.CreateBookingAsync(eventId);
                results.Add(response);
            }

            Assert.Equal(totalSeats, results.Count);
            var ids = results.Select(r => r.Id).Distinct();
            Assert.Equal(totalSeats, ids.Count()); // Все Id уникальны
            Assert.Equal(0, eventEntity.AvailableSeats); // все места заняты
            _taskStoreMock.Verify(x => x.Enqueue(It.IsAny<Booking>()), Times.Exactly(totalSeats));
        }

        [Fact]
        public async Task CreateBookingAsync_WhenNoSeatsLeft_ThrowsNoAvailableSeatsException()
        {
            // Arrange
            int eventId = 1;
            var eventEntity = new Event(1) { Id = eventId }; // только 1 место

            _eventStoreMock
                .Setup(x => x.TryGetEventById(eventId, out It.Ref<Event?>.IsAny))
                .Returns((int id, out Event? ev) =>
                {
                    ev = eventEntity;
                    return true;
                });

            // Act
            await _bookingService.CreateBookingAsync(eventId);

            // Assert 
            await Assert.ThrowsAsync<NoAvailableSeatsException>(async () => await _bookingService.CreateBookingAsync(eventId));

            _taskStoreMock.Verify(x => x.Enqueue(It.IsAny<Booking>()), Times.Once);
        }
    }
}
