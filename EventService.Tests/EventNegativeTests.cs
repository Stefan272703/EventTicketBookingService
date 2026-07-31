using EventTicketBookingService.Models;
using System.ComponentModel.DataAnnotations;

namespace EventService.Tests
{
    public class EventNegativeTests
    {
        // Получение несуществующего события
        [Fact]
        public void GetEventById_NonExistingId_ReturnsNull()
        {
            // Arrange
            var eventService = new EventTicketBookingService.Services.EventService();
            // Act
            var result = eventService.GetEventById(999);

            // Assert
            Assert.Null(result);
        }

        // Обновление несуществующего события
        [Fact]
        public void UpdateEvent_NonExistingId_ReturnsNull()
        {
            // Arrange
            var eventService = new EventTicketBookingService.Services.EventService();


            var updateData = new Event
            {
                Title = "New Title",
                Description = "New Desc",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(1)
            };

            // Act
            var result = eventService.UpdateEvent(999, updateData);

            // Assert
            Assert.Null(result);
        }
        // Удалание несуществующего события
        [Fact]
        public void DeleteEvent_NonExistingId_ReturnsNull()
        {
            // Arrange
            var eventService = new EventTicketBookingService.Services.EventService();

            // Act
            var result = eventService.DeleteEvent(999);

            // Assert
            Assert.Null(result);
        }

        // Создание события с пустым названием
        [Fact]
        public void CreateEvent_EmptyTitle_ThrowsArgumentException()
        {
            // Arrange
            var eventService = new EventTicketBookingService.Services.EventService();

            var invalidEvent = new Event
            {
                Title = "",                     // пустое название
                Description = "Desc",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(1)
            };

            // Act & Assert
            Assert.Throws<ValidationException>(() => eventService.CreateEvent(invalidEvent));
        }

        // Создание события с EndAt раньше StartAt
        [Fact]
        public void CreateEvent_EndAtBeforeStartAt_ThrowsArgumentException()
        {
            // Arrange
            var eventService = new EventTicketBookingService.Services.EventService();

            var invalidEvent = new Event
            {
                Title = "Invalid dates",
                Description = "Desc",
                StartAt = DateTime.Now.AddHours(2),
                EndAt = DateTime.Now              // EndAt раньше StartAt
            };

            // Act & Assert
            Assert.Throws<ValidationException>(() => eventService.CreateEvent(invalidEvent));
        }

        // Обновление события с некорректными датами 
        [Fact]
        public void UpdateEvent_EndAtBeforeStartAt_ThrowsArgumentException()
        {
            // Arrange – сначала создаём корректное событие
            var eventService = new EventTicketBookingService.Services.EventService();

            var validEvent = new Event
            {
                Title = "Valid",
                Description = "Desc",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(1)
            };
            var created = eventService.CreateEvent(validEvent);
            int id = created.Id;

            // Подготавливаем обновление с некорректными датами
            var invalidUpdate = new Event
            {
                Title = "Updated",
                Description = "Updated desc",
                StartAt = DateTime.Now.AddHours(3),
                EndAt = DateTime.Now.AddHours(1)  // EndAt раньше StartAt
            };

            // Act & Assert
            Assert.Throws<ValidationException>(() => eventService.UpdateEvent(id, invalidUpdate));
        }

    }
}
