using EventTicketBookingService.DataAccess;
using EventTicketBookingService.Exceptions;
using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookingService.Tests
{
    public class BookingServiceConcurrencyTests
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;

        public BookingServiceConcurrencyTests()
        {
            //_taskStoreMock = new Mock<IBookingTaskQueue>();
            //_eventStoreMock = new Mock<IEventStore>();
            //_bookingService = new EventTicketBookingService.Services.BookingService(_taskStoreMock.Object,
            //                                                                        _eventStoreMock.Object);

            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
            services.AddScoped<IEventService, EventTicketBookingService.Services.EventService>();
            services.AddScoped<IBookingService, EventTicketBookingService.Services.BookingService>();

            _serviceProvider = services.BuildServiceProvider();
            _scope = _serviceProvider.CreateScope();
            _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
            _bookingService = _scope.ServiceProvider.GetRequiredService<IBookingService>();
        }

        public void Dispose()
        {
            _scope.Dispose();
            _serviceProvider.Dispose();
        }

        private async Task<int> CreateTestEventAsync(int totalSeats = 10)
        {
            var futureDate = DateTime.UtcNow.AddDays(1);
            var created = await _eventService.CreateEventAsync(new EventInfo
            {
                Title = "Test Event",
                StartAt = futureDate,
                EndAt = futureDate.AddHours(2),
                TotalSeats = totalSeats
            });
            return created.Id;
        }

        private async Task<Event> GetEventAsync(int eventId)
        {
            //using var scope = _serviceProvider.CreateScope();
            var context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var @event = await context.Events.FirstAsync(e => e.Id == eventId);
            return @event;
        }

        [Fact]
        public async Task ConcurrentBooking_Overbooking_Exactly5SuccessAnd15Failures()
        {
            //int eventId = 1;
            int totalSeats = 5;
            int requests = 20;

            //var eventEntity = new Event(totalSeats) { Id = eventId };
            var eventId = await CreateTestEventAsync(totalSeats);
            //var @event = await GetEventAsync(eventId);

            //_eventStoreMock
            //    .Setup(x => x.TryGetEventById(eventId, out It.Ref<Event?>.IsAny))
            //    .Returns((int id, out Event? ev) =>
            //    {
            //        ev = eventEntity;
            //        return true;
            //    });

            // Act - запускаем параллельные запросы
            var tasks = Enumerable.Range(0, requests)
                .Select(_ => Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    try
                    {
                        await bookingService.CreateBookingAsync(eventId);
                        return true;
                    }
                    catch (NoAvailableSeatsException)
                    {
                        return false;
                    }
                }
                ));
                //.ToList();
            //.Select(_ => _bookingService.CreateBookingAsync(@event.Id))
            //.ToList();

            var results = await Task.WhenAll(tasks);
            var successCount = results.Count(r => r);
            Assert.Equal(totalSeats, successCount);
            //var results = await Task.WhenAll(tasks.Select(t => t.ContinueWith(tr =>
            //{
            //    if (tr.IsFaulted)
            //    {
            //        var ex = tr.Exception?.InnerException;
            //        if (ex is NoAvailableSeatsException)
            //            return (Success: false, Exception: ex);
            //        throw ex!;
            //    }
            //    return (Success: true, Exception: null);
            //}, TaskContinuationOptions.ExecuteSynchronously)));

            // Assert
            //var successful = results.Count(r => r.Success);
            //var failures = results.Count(r => r.Exception is NoAvailableSeatsException);

            //Assert.Equal(totalSeats, successful);
            //Assert.Equal(requests - totalSeats, failures);
            //Assert.Equal(0, @event.AvailableSeats);

            //_taskStoreMock.Verify(x => x.Enqueue(It.IsAny<Booking>()), Times.Exactly(totalSeats));
        }

        [Fact]
        public async Task ConcurrentBooking_UniqueIdsGuaranteed()
        {
            // Arrange
            //const int eventId = 1;
            const int totalSeats = 10;
            const int requests = 10;
            var eventId = await CreateTestEventAsync(totalSeats);
            var bookingIds = new System.Collections.Concurrent.ConcurrentBag<int>();
            //var eventEntity = new Event(totalSeats) { Id = eventId };

            //_eventStoreMock
            //    .Setup(x => x.TryGetEventById(eventId, out It.Ref<Event?>.IsAny))
            //    .Returns((int id, out Event? ev) =>
            //    {
            //        ev = eventEntity;
            //        return true;
            //    });

            // Act - запускаем 10 параллельных запросов
            var tasks = Enumerable.Range(0, requests)
                .Select(_ => Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                    var booking = await bookingService.CreateBookingAsync(eventId);
                    bookingIds.Add(booking.Id);
                }));
                //.Select(_ => _bookingService.CreateBookingAsync(eventId))
                //.ToList();

            await Task.WhenAll(tasks);

            // Assert
            //var ids = responses.Select(r => r.Id).ToList();
            //Assert.Equal(totalSeats, ids.Count);
            Assert.Equal(totalSeats, bookingIds.Distinct().Count()); // все уникальны

            //_taskStoreMock.Verify(x => x.Enqueue(It.IsAny<Booking>()), Times.Exactly(totalSeats));
        }

    }
}
