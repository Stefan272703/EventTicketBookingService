using EventTicketBookingService.DataAccess;
using EventTicketBookingService.Exceptions;
using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EventService.Tests
{
    public class EventServiceTests : IDisposable
    {
        //private readonly Mock<IEventStore> _eventStoreMock;
        private readonly IEventService _eventService;
        private readonly ServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;

        public EventServiceTests()
        {
            //_eventStoreMock = new Mock<IEventStore>();
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
            services.AddScoped<IEventService, EventTicketBookingService.Services.EventService>();

            _serviceProvider = services.BuildServiceProvider();
            _scope = _serviceProvider.CreateScope();
            _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
            //_eventService = new EventTicketBookingService.Services.EventService(_eventStoreMock.Object);
        }

        public void Dispose()
        {
            _scope.Dispose();
            _serviceProvider.Dispose();
        }

        // Метод теста создания события при валидных данных
        [Fact]
        public async Task CreateEventAsync_ValidEvent_ReturnsCreatedEvent()
        {
            // Arrange
            var testEvent = new EventInfo()
            {
                Id = 1,
                Title = "Белоснежка",
                Description = "Description",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(2),
                TotalSeats = 100
            };

            // Act
            var result = await _eventService.CreateEventAsync(testEvent);

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
        public async Task GetEvents_ExistedEvents_ReturnsAllEvents()
        {
            // Arrange
            await _eventService?.CreateEventAsync(new EventInfo
            {
                Title = "Белоснежка",
                Description = "Сказка",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(2),
                TotalSeats = 100
            });

            await _eventService?.CreateEventAsync(new EventInfo
            {
                Title = "Король и шут",
                Description = "Панк-рок",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(3),
                TotalSeats = 100
            });


            // Act
            var result = await _eventService.GetAllEventsAsync("", null, null, 1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Events.Count());
        }

        [Fact]
        public async Task GetEvent_ExistedEventByID_ReturnsEventById()
        {
            // Arrange
            var expectedEvent = new EventInfo
            {
                Title = "Белоснежка",
                Description = "Сказка",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(2),
                TotalSeats = 100
            };
            await _eventService.CreateEventAsync(expectedEvent);

            await _eventService.CreateEventAsync(new EventInfo
            {
                Title = "Король и шут",
                Description = "Панк-рок",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(3),
                TotalSeats = 100
            });


            // Act
            var result = await _eventService.GetEventByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(expectedEvent.Title, result.Title);
            Assert.Equal(expectedEvent.Description, result.Description);
            Assert.Equal(expectedEvent.StartAt, result.StartAt);
            Assert.Equal(expectedEvent.EndAt, result.EndAt);

        }
        [Fact]
        public async Task UpdateEvent_ExistingId_UpdatesFieldsAndReturnsUpdatedDto()
        {
            // Arrange

            // Исходное событие
            var originalEvent = new EventInfo
            {
                Title = "Белоснежка",
                Description = "Сказка",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(2),
                TotalSeats = 100
            };
            var created = await _eventService.CreateEventAsync(originalEvent);
            int id = created.Id;

            // Новые данные
            var updatedData = new EventInfo
            {
                Title = "Белоснежка часть 2",
                Description = "Фентези",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(3),
                TotalSeats = 100,
            };

            // Act
            var result = await _eventService.UpdateEventAsync(id, updatedData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal(updatedData.Title, result.Title);
            Assert.Equal(updatedData.Description, result.Description);
            Assert.Equal(updatedData.StartAt, result.StartAt);
            Assert.Equal(updatedData.EndAt, result.EndAt);

            // Дополнительно проверка, что в in-memory все сохранилось
            var savedEvent = await _eventService.GetEventByIdAsync(id);
            Assert.NotNull(savedEvent);
            Assert.Equal(updatedData.Title, savedEvent.Title);
        }

        [Fact]
        public async Task DeleteEvent_ExistingId_RemovesEventAndThrowsResourceNotFoundDeletedDto()
        {
            // Arrange
            // Создаём событие
            var created = await _eventService.CreateEventAsync(new EventInfo
            {
                Title = "Noname",
                Description = "No Description",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(2),
                TotalSeats = 100
            });
            int id = created.Id;

            // Act
            var result = await _eventService.DeleteEventAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal("Noname", result.Title);

            // Проверяем, что событие не найдено
            // Act && Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(() => _eventService.GetEventByIdAsync(id));
        }
    }
}
