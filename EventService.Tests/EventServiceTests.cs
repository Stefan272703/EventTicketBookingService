using EventTicketBookingService.Exceptions;
using EventTicketBookingService.Models;

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
            var result = eventService.GetAllEvents("", null, null, 1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Events.Count());
        }

        [Fact]
        public void GetEvent_ExistedEventByID_ReturnsEventById()
        {
            // Arrange
            var eventService = new EventTicketBookingService.Services.EventService();

            var expectedEvent = new Event
            {
                Title = "Белоснежка",
                Description = "Сказка",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(2)
            };
            eventService.CreateEvent(expectedEvent);

            eventService.CreateEvent(new Event
            {
                Title = "Король и шут",
                Description = "Панк-рок",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(3)
            });


            // Act
            var result = eventService.GetEventById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(expectedEvent.Title, result.Title);
            Assert.Equal(expectedEvent.Description, result.Description);
            Assert.Equal(expectedEvent.StartAt, result.StartAt);
            Assert.Equal(expectedEvent.EndAt, result.EndAt);

        }
        [Fact]
        public void UpdateEvent_ExistingId_UpdatesFieldsAndReturnsUpdatedDto()
        {
            // Arrange
            var eventService = new EventTicketBookingService.Services.EventService(); // экземплярный список

            // Исходное событие
            var originalEvent = new Event
            {
                Title = "Белоснежка",
                Description = "Сказка",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(2)
            };
            var created = eventService.CreateEvent(originalEvent);
            int id = created.Id;

            // Новые данные
            var updatedData = new Event
            {
                Title = "Белоснежка часть 2",
                Description = "Фентези",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(3)
            };

            // Act
            var result = eventService.UpdateEvent(id, updatedData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal(updatedData.Title, result.Title);
            Assert.Equal(updatedData.Description, result.Description);
            Assert.Equal(updatedData.StartAt, result.StartAt);
            Assert.Equal(updatedData.EndAt, result.EndAt);

            // Дополнительно проверка, что в in-memory все сохранилось
            var savedEvent = eventService.GetEventById(id);
            Assert.NotNull(savedEvent);
            Assert.Equal(updatedData.Title, savedEvent.Title);
        }

        [Fact]
        public void DeleteEvent_ExistingId_RemovesEventAndThrowsResourceNotFoundDeletedDto()
        {
            // Arrange
            var eventService = new EventTicketBookingService.Services.EventService(); // экземплярный список

            // Создаём событие
            var created = eventService.CreateEvent(new Event
            {
                Title = "Noname",
                Description = "No Description",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(2)
            });
            int id = created.Id;

            // Act
            var result = eventService.DeleteEvent(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal("Noname", result.Title);

            // Проверяем, что событие не найдено
            // Act && Assert
            Assert.Throws<ResourceNotFoundException>(() => eventService.GetEventById(id));
        }
    }
}
