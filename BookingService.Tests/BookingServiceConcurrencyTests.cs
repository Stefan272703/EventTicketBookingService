using EventTicketBookingService.Exceptions;
using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookingService.Tests
{
    public class BookingServiceConcurrencyTests
    {
        private readonly Mock<IBookingTaskQueue> _taskStoreMock;
        private readonly Mock<IEventStore> _eventStoreMock;
        private readonly EventTicketBookingService.Services.BookingService _bookingService;

        public BookingServiceConcurrencyTests()
        {
            _taskStoreMock = new Mock<IBookingTaskQueue>();
            _eventStoreMock = new Mock<IEventStore>();
            _bookingService = new EventTicketBookingService.Services.BookingService(_taskStoreMock.Object,
                                                                                   _eventStoreMock.Object);
        }

        [Fact]
        public async Task ConcurrentBooking_Overbooking_Exactly5SuccessAnd15Failures()
        {
            int eventId = 1;
            int totalSeats = 5;
            int requests = 20;

            var eventEntity = new Event(totalSeats) { Id = eventId };

            _eventStoreMock
                .Setup(x => x.TryGetEventById(eventId, out It.Ref<Event?>.IsAny))
                .Returns((int id, out Event? ev) =>
                {
                    ev = eventEntity;
                    return true;
                });

            // Act - запускаем параллельные запросы
            var tasks = Enumerable.Range(0, requests)
                .Select(_ => _bookingService.CreateBookingAsync(eventId))
                .ToList();

            var results = await Task.WhenAll(tasks.Select(t => t.ContinueWith(tr =>
            {
                if (tr.IsFaulted)
                {
                    var ex = tr.Exception?.InnerException;
                    if (ex is NoAvailableSeatsException)
                        return (Success: false, Exception: ex);
                    throw ex!;
                }
                return (Success: true, Exception: null);
            },TaskContinuationOptions.ExecuteSynchronously)));

            // Assert
            var successful = results.Count(r => r.Success);
            var failures = results.Count(r => r.Exception is NoAvailableSeatsException);

            Assert.Equal(totalSeats, successful);
            Assert.Equal(requests - totalSeats, failures);
            Assert.Equal(0, eventEntity.AvailableSeats);

            _taskStoreMock.Verify(x => x.Enqueue(It.IsAny<Booking>()), Times.Exactly(totalSeats));
        }

        [Fact]
        public async Task ConcurrentBooking_UniqueIdsGuaranteed()
        {
            // Arrange
            const int eventId = 1;
            const int totalSeats = 10;

            var eventEntity = new Event(totalSeats) { Id = eventId };

            _eventStoreMock
                .Setup(x => x.TryGetEventById(eventId, out It.Ref<Event?>.IsAny))
                .Returns((int id, out Event? ev) =>
                {
                    ev = eventEntity;
                    return true;
                });

            // Act - запускаем 10 параллельных запросов
            var tasks = Enumerable.Range(0, totalSeats)
                .Select(_ => _bookingService.CreateBookingAsync(eventId))
                .ToList();

            var responses = await Task.WhenAll(tasks);

            // Assert
            var ids = responses.Select(r => r.Id).ToList();
            Assert.Equal(totalSeats, ids.Count);
            Assert.Equal(totalSeats, ids.Distinct().Count()); // все уникальны

            _taskStoreMock.Verify(x => x.Enqueue(It.IsAny<Booking>()), Times.Exactly(totalSeats));
        }

    }
}
