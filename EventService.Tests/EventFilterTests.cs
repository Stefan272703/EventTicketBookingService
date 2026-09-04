
using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using Moq;

namespace EventService.Tests
{
    public class EventFilterTests
    {
        private readonly Mock<IEventStore> _eventStoreMock;
        private readonly EventTicketBookingService.Services.EventService _eventService;

        public EventFilterTests()
        {
            _eventStoreMock = new Mock<IEventStore>();
            _eventService = new EventTicketBookingService.Services.EventService(_eventStoreMock.Object);
        }

        // Фильтрация событий по названию
        [Fact]
        public async Task GetAllEvents_FilterByTitle_ReturnsEventsWithMatchingSubstring()
        {
            // Arrange
            await _eventService.CreateEventAsync(new EventInfo { Title = "Белоснежка", StartAt = DateTime.Now, EndAt = DateTime.Now.AddHours(1), TotalSeats = 100});
            await _eventService.CreateEventAsync(new EventInfo { Title = "Король и шут", StartAt = DateTime.Now, EndAt = DateTime.Now.AddHours(2), TotalSeats = 200});
            await _eventService.CreateEventAsync(new EventInfo { Title = "Белое солнце пустыни", StartAt = DateTime.Now, EndAt = DateTime.Now.AddHours(3), TotalSeats = 250});

            // Act (Фильтрация по названию)
            var result = _eventService.GetAllEvents("бел", DateTime.MinValue, DateTime.MaxValue, 1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Events.Count());
            Assert.All(result.Events, e => Assert.Contains("бел", e.Title.ToLower()));
        }

        // Фильтрация по StartAt
        [Fact]
        public async Task GetAllEvents_FilterByStartAt_ReturnsEventsStartingAfterOrAtDate()
        {
            // Arrange
            var now = DateTime.Now;
            var past = now.AddDays(-1);
            var future = now.AddDays(1);

            await _eventService.CreateEventAsync(new EventInfo { Title = "Past", StartAt = past, EndAt = past.AddHours(1), TotalSeats = 100});
            await _eventService.CreateEventAsync(new EventInfo { Title = "Now", StartAt = now, EndAt = now.AddHours(1), TotalSeats=200});
            await _eventService.CreateEventAsync(new EventInfo { Title = "Future", StartAt = future, EndAt = future.AddHours(1), TotalSeats= 250 });

            // Act – ищем события с StartAt >= now
            var result = _eventService.GetAllEvents("", now, DateTime.MaxValue, 1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Events.Count());
            Assert.DoesNotContain(result.Events, e => e.Title == "Past");
        }

        // Фильтрация по EndAt
        [Fact]
        public async Task GetAllEvents_FilterByEndAt_ReturnsEventsEndingBeforeOrAtDate()
        {
            // Arrange
            var now = DateTime.Now;
            var earlier = now.AddHours(-2);
            var later = now.AddHours(2);

            await _eventService.CreateEventAsync(new EventInfo { Title = "Earlier", StartAt = earlier, EndAt = earlier.AddHours(1), TotalSeats=100 }); // закончится до now
            await _eventService.CreateEventAsync(new EventInfo { Title = "Now", StartAt = now, EndAt = now.AddHours(1), TotalSeats=200 }); // закончится после now
            await _eventService.CreateEventAsync(new EventInfo { Title = "Later", StartAt = later, EndAt = later.AddHours(1), TotalSeats=250 }); // закончится после now

            // Act – ищем события с EndAt <= now
            var result = _eventService.GetAllEvents("", DateTime.MinValue, now, 1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Events);
            Assert.Equal("Earlier", result.Events.First().Title);
        }

        // Комбинированная фильтрация (диапазон)
        [Fact]
        public async Task GetAllEvents_FilterByDateRange_ReturnsEventsWithinRange()
        {
            // Arrange
            var date1 = new DateTime(2026, 7, 31, 10, 0, 0);
            var date2 = new DateTime(2026, 7, 31, 12, 0, 0);
            var date3 = new DateTime(2026, 7, 31, 14, 0, 0);
            var date4 = new DateTime(2026, 7, 31, 16, 0, 0);

            await _eventService.CreateEventAsync(new EventInfo { Title = "A", StartAt = date1, EndAt = date1.AddHours(1), TotalSeats = 100 }); // внутри
            await _eventService.CreateEventAsync(new EventInfo { Title = "B", StartAt = date2, EndAt = date2.AddHours(1), TotalSeats = 200 }); // внутри (граница)
            await _eventService.CreateEventAsync(new EventInfo { Title = "C", StartAt = date4, EndAt = date4.AddHours(1), TotalSeats = 250 }); // за пределами

            // Act – диапазон [date1, date3]
            var result = _eventService.GetAllEvents("", date1, date3, 1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Events.Count());
            Assert.Contains(result.Events, e => e.Title == "A");
            Assert.Contains(result.Events, e => e.Title == "B");
            Assert.DoesNotContain(result.Events, e => e.Title == "C");
        }
    }
}
