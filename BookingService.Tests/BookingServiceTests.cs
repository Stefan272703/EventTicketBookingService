using EventTicketBookingService.DataAccess;
using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace BookingService.Tests
{
    public class BookingServiceTests
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;

        public BookingServiceTests()
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

        // Создание брони для существующего события — возвращается BookingInfo со статусом Pending;
        [Fact]
        public async Task CreateBookingAsync_ExistingEvent_ReturnsBookingInfoByPending()
        {
            // Arrange
            var eventId = await CreateTestEventAsync();

            //const int eventId = 1;
            //_eventStoreMock.Setup(x => x.TryGetEventById(eventId, out It.Ref<Event?>.IsAny))
            //               .Returns((int id, out Event? ev) =>
            //               {
            //                   ev = new Event(5) { Id = id };
            //                   return true;
            //               });
            // Act
            var result = await _bookingService.CreateBookingAsync(eventId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(BookingStatus.Pending, result.Status);
            //_taskStoreMock.Verify(x => x.Enqueue(It.IsAny<Booking>()), Times.Once);
        }

        // Создание нескольких броней для одного события — все создаются с уникальными Id
        [Fact]
        public async Task CreateBookingAsync_MiltipleBookingsForSameEvent_GenerateUniqueIds()
        {
            // Arrange
            var eventId = await CreateTestEventAsync();
            //const int eventId = 1;
            //_eventStoreMock.Setup(x => x.TryGetEventById(eventId, out It.Ref<Event?>.IsAny))
            //   .Returns((int id, out Event? ev) =>
            //   {
            //       ev = new Event(5) { Id = id };
            //       return true;
            //   });


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
            var eventId = await CreateTestEventAsync();
            //const int eventId = 1;
            //_eventStoreMock.Setup(x => x.TryGetEventById(eventId, out It.Ref<Event?>.IsAny))
            //   .Returns((int id, out Event? ev) =>
            //   {
            //       ev = new Event(5) { Id = id };
            //       return true;
            //   });

            var created = await _bookingService.CreateBookingAsync(eventId);

            // Act
            var result = await _bookingService.GetBookingByIdAsync(created.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(result.Id, created.Id);
            Assert.Equal(eventId, result.EventId);
            Assert.Equal(BookingStatus.Pending, result.Status);
        }

        // Получение брони отражает изменение статуса (после Confirm/Reject).
        [Fact]
        public async Task GetBookingByIdAsync_AfterStatusChange_ReflectsUpdatedStatus()
        {
            // Arrange
            var eventId = await CreateTestEventAsync();
            //const int eventId = 1;
            //_eventStoreMock.Setup(x => x.TryGetEventById(eventId, out It.Ref<Event?>.IsAny))
            //   .Returns((int id, out Event? ev) =>
            //   {
            //       ev = new Event(5) { Id = id };
            //       return true;
            //   });

            var created = await _bookingService.CreateBookingAsync(eventId);
            var booking = await _bookingService.GetBookingByIdAsync(created.Id);
            booking.Confirm();
            var bookingId = created.Id;

            // Act
            //await _bookingService.UpdateBookingStatusAsync(bookingId, BookingStatus.Confirmed, CancellationToken.None);

            var updated = await _bookingService.GetBookingByIdAsync(bookingId);

            // Assert
            Assert.NotNull(updated);
            Assert.Equal(BookingStatus.Confirmed, updated.Status);
            Assert.NotNull(updated.ProcessedAt);
        }
    }
}
