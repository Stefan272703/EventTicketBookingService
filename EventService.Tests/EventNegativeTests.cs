using EventTicketBookingService.Exceptions;
using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace EventService.Tests
{
    public class EventNegativeTests
    {
        private readonly Mock<IEventStore> _eventStoreMock;
        private readonly EventTicketBookingService.Services.EventService _eventService;

        public EventNegativeTests()
        {
            _eventStoreMock = new Mock<IEventStore>();
            _eventService = new EventTicketBookingService.Services.EventService(_eventStoreMock.Object);
        }

        // Получение несуществующего события
        [Fact]
        public void GetEventById_NonExistingId_ThrowsResourceNotFoundException()
        {
            // Act && Assert
            Assert.Throws<ResourceNotFoundException>(() => _eventService.GetEventById(999));
        }

        // Обновление несуществующего события
        [Fact]
        public void UpdateEvent_NonExistingId_ThrowsResourceNotFoundException()
        {
            // Arrange
            var updateData = new EventInfo
            {
                Title = "New Title",
                Description = "New Desc",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(1)
            };

            // Act && Assert
            Assert.Throws<ResourceNotFoundException>(() => _eventService.UpdateEvent(999, updateData));
        }
        // Удалание несуществующего события
        [Fact]
        public void DeleteEvent_NonExistingId_ThrowsResourceNotFoundException()
        {
            // Act && Assert
            Assert.Throws<ResourceNotFoundException>(() => _eventService.DeleteEvent(999));
        }

        // Создание события с пустым названием
        [Fact]
        public async Task CreateEvent_EmptyTitle_ThrowsArgumentException()
        {
            // Arrange
            var invalidEvent = new EventInfo
            {
                Title = "",                     // пустое название
                Description = "Desc",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(1)
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _eventService.CreateEventAsync(invalidEvent));
        }

        // Создание события с EndAt раньше StartAt
        [Fact]
        public async Task CreateEvent_EndAtBeforeStartAt_ThrowsArgumentException()
        {
            // Arrange
            var invalidEvent = new EventInfo
            {
                Title = "Invalid dates",
                Description = "Desc",
                StartAt = DateTime.Now.AddHours(2),
                EndAt = DateTime.Now              // EndAt раньше StartAt
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _eventService.CreateEventAsync(invalidEvent));
        }

        // Обновление события с некорректными датами 
        [Fact]
        public async Task UpdateEvent_EndAtBeforeStartAt_ThrowsArgumentException()
        {
            // Arrange – сначала создаём корректное событие
            var validEvent = new EventInfo
            {
                Title = "Valid",
                Description = "Desc",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(1),
                TotalSeats = 1
            };
            var created = await _eventService.CreateEventAsync(validEvent);
            int id = created.Id;

            // Подготавливаем обновление с некорректными датами
            var invalidUpdate = new EventInfo
            {
                Title = "Updated",
                Description = "Updated desc",
                StartAt = DateTime.Now.AddHours(3),
                EndAt = DateTime.Now.AddHours(1)  // EndAt раньше StartAt
            };

            // Act & Assert
            Assert.Throws<ValidationException>(() => _eventService.UpdateEvent(id, invalidUpdate));
        }

    }
}
