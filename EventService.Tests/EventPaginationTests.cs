using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using Moq;

namespace EventService.Tests
{
    public class EventPaginationTests
    {
        private readonly Mock<IEventStore> _eventStoreMock;
        private readonly EventTicketBookingService.Services.EventService _eventService;

        public EventPaginationTests()
        {
            _eventStoreMock = new Mock<IEventStore>();
            _eventService = new EventTicketBookingService.Services.EventService(_eventStoreMock.Object);
        }

        // Первая страница (page=1, pageSize=2)
        [Fact]
        public async Task GetAllEvents_Pagination_FirstPage_ReturnsFirstTwoEvents()
        {
            // Arrange
            for (int i = 1; i <= 5; i++)
            {
                await _eventService.CreateEventAsync(new EventInfo
                {
                    Title = $"Event {i}",
                    StartAt = DateTime.Now.AddHours(i),
                    EndAt = DateTime.Now.AddHours(i + 1),
                    TotalSeats= 100,
                });
            }

            // Act
            var result = _eventService.GetAllEvents("", DateTime.MinValue, DateTime.MaxValue, 1, 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(2, result.Events.Count());
            Assert.Equal(1, result.PageIndex);
            Assert.Equal(2, result.PageSizeByIndex);

            var titles = result.Events.Select(e => e.Title).ToList();
            Assert.Contains("Event 1", titles);
            Assert.Contains("Event 2", titles);
            Assert.DoesNotContain("Event 3", titles);
        }

        // Вторая страница(page = 2, pageSize = 2)
        [Fact]
        public async Task GetAllEvents_Pagination_SecondPage_ReturnsNextTwoEvents()
        {
            // Arrange
            for (int i = 1; i <= 5; i++)
            {
                await _eventService.CreateEventAsync(new EventInfo
                {
                    Title = $"Event {i}",
                    StartAt = DateTime.Now.AddHours(i),
                    EndAt = DateTime.Now.AddHours(i + 1),
                    TotalSeats = 100
                });
            }

            // Act
            var result = _eventService.GetAllEvents("", DateTime.MinValue, DateTime.MaxValue, 2, 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(2, result.Events.Count());
            Assert.Equal(2, result.PageIndex);
            Assert.Equal(2, result.PageSizeByIndex);

            var titles = result.Events.Select(e => e.Title).ToList();
            Assert.Contains("Event 3", titles);
            Assert.Contains("Event 4", titles);
            Assert.DoesNotContain("Event 1", titles);
            Assert.DoesNotContain("Event 5", titles);
        }

        // Последняя страница (остаток меньше pageSize)
        // При 5 элементах и pageSize = 2, последняя страница – это page = 3.
        [Fact]
        public async Task GetAllEvents_Pagination_LastPage_ReturnsRemainingEvents()
        {
            // Arrange
            for (int i = 1; i <= 5; i++)
            {
                await _eventService.CreateEventAsync(new EventInfo
                {
                    Title = $"Event {i}",
                    StartAt = DateTime.Now.AddHours(i),
                    EndAt = DateTime.Now.AddHours(i + 1),
                    TotalSeats = 100
                });
            }

            // Act
            var result = _eventService.GetAllEvents("", DateTime.MinValue, DateTime.MaxValue, 3, 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.TotalCount);
            Assert.Single(result.Events);            // только одно событие (Event 5)
            Assert.Equal(3, result.PageIndex);
            Assert.Equal(1, result.PageSizeByIndex); // фактически 1 элемент на странице

            var title = result.Events.First().Title;
            Assert.Equal("Event 5", title);
        }
    }
}
