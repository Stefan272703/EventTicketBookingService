using BuildingBlocks.Domain;
using BuildingBlocks.Domain.Exceptions;
using EventService.Application.DTO;
using EventService.Application.Interfaces;
using EventService.Application.Interfaces.Redis;
using EventService.Application.Repository.Interface;
using EventService.Domain.Models;
using EventService.Infrastructure.DataAccess;
using EventService.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EventService.Tests
{
    public class EventServiceCacheTests
    {
        private readonly IEventService _eventService;
        private readonly ServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;
        private readonly Mock<ICacheService> _cacheMock;
        private readonly AppDbContext _db;


        public EventServiceCacheTests()
        {
            var dbName = Guid.NewGuid().ToString();
            _cacheMock = new Mock<ICacheService>(MockBehavior.Strict);

            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IEventService, EventService.Application.Services.EventService>();
            services.AddSingleton<ICacheService>(_cacheMock.Object);
            services.Configure<RedisOptions>(o => { o.TTL = 30;});

            _serviceProvider = services.BuildServiceProvider();
            _scope = _serviceProvider.CreateScope();
            _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
            _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        }

        public void Dispose()
        {
            _scope.Dispose();
            _serviceProvider.Dispose();
        }

        // Попадание в кеш
        [Fact]
        public async Task GetEventByIdAsync_WhenCacheHit_ReturnsCachedValue_NotDbValue()
        {
            // Arrange: в БД одно, в кеше другое — кто ответит, тот и «победил»
            var evt = await SeedEventAsync("DB Title");
            var cached = new EventInfo { Id = evt.Id, Title = "Cached Title" };

            _cacheMock
                .Setup(c => c.GetAsync<EventInfo>(Key(evt.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cached);

            // Act
            var result = await _eventService.GetEventByIdAsync(evt.Id);

            // Assert: вернулось значение из кеша, а не из БД
            Assert.NotNull(result);
            Assert.Equal("Cached Title", result!.Title);

            // SetAsync НЕ вызывался — при hit нечего записывать
            _cacheMock.Verify(c => c.SetAsync(
                It.IsAny<string>(), It.IsAny<EventInfo>(),
                It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // Промах кеша
        [Fact]
        public async Task GetEventByIdAsync_WhenCacheMiss_ReadsFromDb_AndWritesToCache()
        {
            // Arrange
            var evt = await SeedEventAsync("DB Title");

            _cacheMock
                .Setup(c => c.GetAsync<EventInfo>(Key(evt.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EventInfo?)null);

            _cacheMock
                .Setup(c => c.SetAsync(
                    Key(evt.Id),
                    It.Is<EventInfo>(e => e.Title == "DB Title"),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _eventService.GetEventByIdAsync(evt.Id);

            // Assert: пришло из БД, и записалось в кеш тем же значением
            Assert.NotNull(result);
            Assert.Equal("DB Title", result!.Title);

            _cacheMock.Verify(c => c.SetAsync(
                Key(evt.Id),
                It.Is<EventInfo>(e => e.Title == "DB Title"),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        // Выброс исключения, когда нет события, без сохранения в кеш
        [Fact]
        public async Task GetEventByIdAsync_WhenNotFound_Throws_AndDoesNotCacheNull()
        {
            // Arrange
            _cacheMock
                .Setup(c => c.GetAsync<EventInfo>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EventInfo?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(() => _eventService.GetEventByIdAsync(999));

            // Не кешируем "не найдено" — иначе после создания события клиент получит 404 из кеша
            _cacheMock.Verify(c => c.SetAsync(
                It.IsAny<string>(), It.IsAny<EventInfo>(),
                It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // Мутация. Инвалидация вызвана один раз при обновлении события
        [Fact]
        public async Task UpdateEventAsync_InvalidatesCacheKey()
        {
            // Arrange
            var evt = await SeedEventAsync("Old Title");

            _cacheMock
                .Setup(c => c.RemoveKeyAsync(Key(evt.Id), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var update = new EventInfo
            {
                Title = "New Title",
                Description = "new desc",
                StartAt = DateTime.UtcNow.AddDays(3),
                EndAt = DateTime.UtcNow.AddDays(4)
            };

            // Act
            await _eventService.UpdateEventAsync(evt.Id, update);

            // Assert: инвалидация вызвана ровно один раз с правильным ключом
            _cacheMock.Verify(c => c.RemoveKeyAsync(
                Key(evt.Id), It.IsAny<CancellationToken>()), Times.Once);

            // И в БД действительно новое значение
            var fromDb = await _db.Events.AsNoTracking().FirstAsync(e => e.Id == evt.Id);
            Assert.Equal("New Title", fromDb.Title);
        }

        // Мутация. В ChangeTracker не должно быть несохраненных изменений
        [Fact]
        public async Task UpdateEventAsync_SavesToDb_BeforeInvalidatingCache()
        {
            // Arrange
            var evt = await SeedEventAsync("Old Title");
            var dbWasSavedWhenCacheInvalidated = false;

            _cacheMock
                .Setup(c => c.RemoveKeyAsync(Key(evt.Id), It.IsAny<CancellationToken>()))
                .Callback(() =>
                {
                    // В момент инвалидации в ChangeTracker не должно быть несохранённых изменений,
                    // то есть SaveChanges уже отработал
                    dbWasSavedWhenCacheInvalidated = !_db.ChangeTracker.HasChanges();
                })
                .Returns(Task.CompletedTask);

            var update = new EventInfo
            {
                Title = "New Title",
                Description = "new desc",
                StartAt = DateTime.UtcNow.AddDays(3),
                EndAt = DateTime.UtcNow.AddDays(4)
            };

            // Act
            await _eventService.UpdateEventAsync(evt.Id, update);

            // Assert
            Assert.True(dbWasSavedWhenCacheInvalidated);
        }

        // Мутация. Удаление события и инвалидаия кеша
        [Fact]
        public async Task DeleteEventAsync_InvalidatesCacheKey()
        {
            // Arrange
            var evt = await SeedEventAsync();

            _cacheMock
                .Setup(c => c.RemoveKeyAsync(Key(evt.Id), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _eventService.DeleteEventAsync(evt.Id);

            // Assert
            _cacheMock.Verify(c => c.RemoveKeyAsync(
                Key(evt.Id), It.IsAny<CancellationToken>()), Times.Once);

            // В БД события больше нет
            var fromDb = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == evt.Id);
            Assert.Null(fromDb);
        }

        // Мутация. Обновление события, когда событие не найдено, не инвалидируем кеш
        [Fact]
        public async Task UpdateEventAsync_WhenNotFound_DoesNotInvalidateCache()
        {
            // Arrange: события нет, cache.Verify должны остаться пустыми
            var update = new EventInfo
            {
                Title = "X",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2)
            };

            // Act & Assert
            await Assert.ThrowsAsync<ResourceNotFoundException>(() => _eventService.UpdateEventAsync(999, update));

            _cacheMock.Verify(c => c.RemoveKeyAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        private async Task<Event> SeedEventAsync(string title = "DB Title")
        {
            var evt = new Event(totalSeats: 100);
            evt.Update(title, "desc",
                DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2));
            _db.Events.Add(evt);
            await _db.SaveChangesAsync();
            _db.ChangeTracker.Clear();
            return evt;
        }

        private static string Key(int id) => $"event:{id}";
    }
}
