
using EventTicketBookingService.Models;

namespace EventService.Tests
{
    public class EventFilterTests
    {
        // Фильтрация событий по названию
        [Fact]
        public void GetAllEvents_FilterByTitle_ReturnsEventsWithMatchingSubstring()
        {
            // Arrange
            var eventService = new EventTicketBookingService.Services.EventService();

            eventService.CreateEvent(new Event { Title = "Белоснежка", StartAt = DateTime.Now, EndAt = DateTime.Now.AddHours(1)});
            eventService.CreateEvent(new Event { Title = "Король и шут", StartAt = DateTime.Now, EndAt = DateTime.Now.AddHours(2)});
            eventService.CreateEvent(new Event { Title = "Белое солнце пустыни", StartAt = DateTime.Now, EndAt = DateTime.Now.AddHours(3)});

            // Act (Фильтрация по названию)
            var result = eventService.GetAllEvents("бел", DateTime.MinValue, DateTime.MaxValue, 1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Events.Count());
            Assert.All(result.Events, e => Assert.Contains("бел", e.Title.ToLower()));
        }

        // Фильтрация по StartAt
        [Fact]
        public void GetAllEvents_FilterByStartAt_ReturnsEventsStartingAfterOrAtDate()
        {
            // Arrange
            var eventService = new EventTicketBookingService.Services.EventService();
            var now = DateTime.Now;
            var past = now.AddDays(-1);
            var future = now.AddDays(1);

            eventService.CreateEvent(new Event { Title = "Past", StartAt = past, EndAt = past.AddHours(1) });
            eventService.CreateEvent(new Event { Title = "Now", StartAt = now, EndAt = now.AddHours(1) });
            eventService.CreateEvent(new Event { Title = "Future", StartAt = future, EndAt = future.AddHours(1) });

            // Act – ищем события с StartAt >= now
            var result = eventService.GetAllEvents("", now, DateTime.MaxValue, 1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Events.Count());
            Assert.DoesNotContain(result.Events, e => e.Title == "Past");
        }

        // Фильтрация по EndAt
        [Fact]
        public void GetAllEvents_FilterByEndAt_ReturnsEventsEndingBeforeOrAtDate()
        {
            // Arrange
            var eventService = new EventTicketBookingService.Services.EventService();
            var now = DateTime.Now;
            var earlier = now.AddHours(-2);
            var later = now.AddHours(2);

            eventService.CreateEvent(new Event { Title = "Earlier", StartAt = earlier, EndAt = earlier.AddHours(1) }); // закончится до now
            eventService.CreateEvent(new Event { Title = "Now", StartAt = now, EndAt = now.AddHours(1) }); // закончится после now
            eventService.CreateEvent(new Event { Title = "Later", StartAt = later, EndAt = later.AddHours(1) }); // закончится после now

            // Act – ищем события с EndAt <= now
            var result = eventService.GetAllEvents("", DateTime.MinValue, now, 1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Events);
            Assert.Equal("Earlier", result.Events.First().Title);
        }

        // Комбинированная фильтрация (диапазон)
        [Fact]
        public void GetAllEvents_FilterByDateRange_ReturnsEventsWithinRange()
        {
            // Arrange
            var eventService = new EventTicketBookingService.Services.EventService();
            var date1 = new DateTime(2026, 7, 31, 10, 0, 0);
            var date2 = new DateTime(2026, 7, 31, 12, 0, 0);
            var date3 = new DateTime(2026, 7, 31, 14, 0, 0);
            var date4 = new DateTime(2026, 7, 31, 16, 0, 0);

            eventService.CreateEvent(new Event { Title = "A", StartAt = date1, EndAt = date1.AddHours(1) }); // внутри
            eventService.CreateEvent(new Event { Title = "B", StartAt = date2, EndAt = date2.AddHours(1) }); // внутри (граница)
            eventService.CreateEvent(new Event { Title = "C", StartAt = date4, EndAt = date4.AddHours(1) }); // за пределами

            // Act – диапазон [date1, date3]
            var result = eventService.GetAllEvents("", date1, date3, 1, 10);

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
