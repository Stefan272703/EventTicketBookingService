using EventTicketBookingService.Exceptions;
using EventTicketBookingService.Models;
using System.ComponentModel.DataAnnotations;

namespace EventService.Tests
{
    public class EventNegativeTests
    {
        // Получение несуществующего события
        [Fact]
        public void GetEventById_NonExistingId_ThrowsResourceNotFoundException()
        {
            // Arrange
            var eventService = new EventTicketBookingService.Services.EventService();

            // Act && Assert
            Assert.Throws<ResourceNotFoundException>(() => eventService.GetEventById(999));
        }

        // Обновление несуществующего события
        [Fact]
        public void UpdateEvent_NonExistingId_ThrowsResourceNotFoundException()
        {
            // Arrange
            var eventService = new EventTicketBookingService.Services.EventService();


            var updateData = new EventDTO
            {
                Title = "New Title",
                Description = "New Desc",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(1)
            };

            // Act && Assert
            Assert.Throws<ResourceNotFoundException>(() => eventService.UpdateEvent(999, updateData));
        }
        // Удалание несуществующего события
        [Fact]
        public void DeleteEvent_NonExistingId_ThrowsResourceNotFoundException()
        {
            // Arrange
            var eventService = new EventTicketBookingService.Services.EventService();

            // Act && Assert
            Assert.Throws<ResourceNotFoundException>(() => eventService.DeleteEvent(999));
        }

        // Создание события с пустым названием
        [Fact]
        public void CreateEvent_EmptyTitle_ThrowsArgumentException()
        {
            // Arrange
            var eventService = new EventTicketBookingService.Services.EventService();

            var invalidEvent = new EventDTO
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

            var invalidEvent = new EventDTO
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

            var validEvent = new EventDTO
            {
                Title = "Valid",
                Description = "Desc",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(1)
            };
            var created = eventService.CreateEvent(validEvent);
            int id = created.Id;

            // Подготавливаем обновление с некорректными датами
            var invalidUpdate = new EventDTO
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
