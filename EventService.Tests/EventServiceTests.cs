using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using Moq;

namespace EventService.Tests
{
    public class EventServiceTests
    {
        // Метод теста создания события при валидных данных
        [Fact]
        public void CreateEvent_ValidEvent_ReturnsCreatedEvent()
        {
            // Arrange
            var eventService = new EventTicketBookingService.Services.EventService();

            var testEvent = new Event()
            {
                Id = 1,
                Title = "Белоснежка",
                Description = "Description",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(2),
            };

            // Act
            var result = eventService.CreateEvent(testEvent);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(testEvent.Title, result.Title);
            Assert.Equal(testEvent.Description, result.Description);
            Assert.Equal(testEvent.StartAt, result.StartAt);
            Assert.Equal(testEvent.EndAt, result.EndAt);
        }

        // Метод получения всех событий
        [Fact]
        public void GetEvents_ExistedEvents_ReturnsAllEvents()
        {
            // Arrange
            var eventService = new EventTicketBookingService.Services.EventService();

            eventService.CreateEvent(new Event
            {
                Title = "Белоснежка",
                Description = "Сказка",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(2)
            });

            eventService.CreateEvent(new Event
            {
                Title = "Король и шут",
                Description = "Панк-рок",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(3)
            });


            // Act
            var result = eventService.GetAllEvents("", DateTime.MinValue, DateTime.MinValue, 1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Events.Count());
        }

    }
}
