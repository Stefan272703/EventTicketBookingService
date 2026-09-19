using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookingService.Tests
{
    public class BookingServiceStatusTests
    {
        private readonly Mock<IBookingTaskQueue> _taskStoreMock;
        private readonly Mock<IEventStore> _eventStoreMock;
        private readonly EventTicketBookingService.Services.BookingService _bookingService;

        public BookingServiceStatusTests()
        {
            _taskStoreMock = new Mock<IBookingTaskQueue>();
            _eventStoreMock = new Mock<IEventStore>();
            _bookingService = new EventTicketBookingService.Services.BookingService(_taskStoreMock.Object,
                                                                                   _eventStoreMock.Object);
        }

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
            Assert.InRange(booking.ProcessedAt.Value, DateTime.Now.AddSeconds(-1), DateTime.Now.AddSeconds(1));
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
            Assert.InRange(booking.ProcessedAt.Value, DateTime.Now.AddSeconds(-1), DateTime.Now.AddSeconds(1));
        }

        [Fact]
        public async Task Reject_ReleasesSeatsAndAllowsNewBooking()
        {
            // Arrange
            const int eventId = 1;
            var eventEntity = new Event(1) { Id = eventId };
            Booking? createdBooking = null;

            _eventStoreMock
                .Setup(x => x.TryGetEventById(eventId, out It.Ref<Event?>.IsAny))
                .Returns((int id, out Event? ev) =>
                {
                    ev = eventEntity;
                    return true;
                });

            // Act - создаём бронь (занимаем последнее место)
            var response = await _bookingService.CreateBookingAsync(eventId);
            Assert.Equal(0, eventEntity.AvailableSeats);

            var booking = new Booking
            {
                Id = response.Id,
                EventId = eventId,
                Status = BookingStatus.Pending
            };
            booking.Reject();
            eventEntity.ReleaseSeats(); // освобождаем место

            // После освобождения должно стать 1 свободное место
            Assert.Equal(1, eventEntity.AvailableSeats);

            // Теперь можем создать новую бронь
            var newResponse = await _bookingService.CreateBookingAsync(eventId);
            Assert.NotNull(newResponse);
            Assert.Equal(0, eventEntity.AvailableSeats); // снова занято
            _taskStoreMock.Verify(x => x.Enqueue(It.IsAny<Booking>()), Times.Exactly(2));
        }
    }
}
