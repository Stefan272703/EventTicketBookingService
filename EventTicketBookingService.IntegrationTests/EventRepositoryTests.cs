using EventTicketBookingService.Infrastructure.DataAccess;
using EventTicketBookingService.Domain.Models;
using EventTicketBookingService.Infrastructure.Repository;
using EventTicketBookingService.Application.DTO;
using Microsoft.EntityFrameworkCore;

namespace EventTicketBookingService.IntegrationTests
{
    [Collection(PostgresCollection.Name)]
    public class EventRepositoryTests : IAsyncLifetime
    {
        private readonly PostgresContainerFixture _fixture;
        private AppDbContext _context;
        private EventRepository _eventRepo;

        public EventRepositoryTests(PostgresContainerFixture fixture)
        {
            _fixture = fixture;
        }

        public async Task DisposeAsync() => await _context.DisposeAsync();

        public async Task InitializeAsync()
        {
            await _fixture.ResetDatabaseAsync();
            _context = _fixture.CreateContext();
            _eventRepo = new EventRepository(_context);
        }

        [Fact]
        public async Task GetEventByIdAsync_ExistingId_ReturnsEvent()
        {
            var created = await SeedAsync("Тест", DateTime.UtcNow, DateTime.UtcNow.AddHours(1));

            var result = await _eventRepo.GetEventByIdAsync(created.Id);

            Assert.NotNull(result);
            Assert.Equal(created.Id, result!.Id);
            Assert.Equal("Тест", result.Title);
            Assert.Equal(10, result.TotalSeats);
            Assert.Equal(10, result.AvailableSeats);
        }

        [Fact]
        public async Task GetEventByIdAsync_MissingId_ReturnsNull()
        {
            var result = await _eventRepo.GetEventByIdAsync(9999);
            Assert.Null(result);
        }

        [Fact]
        public async Task AddEventAsync_ThenSave_PersistsAllFieldsInDatabase()
        {
            var startAt = new DateTime(2030, 5, 1, 10, 0, 0, DateTimeKind.Utc);
            var endAt = new DateTime(2030, 5, 1, 12, 0, 0, DateTimeKind.Utc);
            var e = new Event(50)
            {
                Id = await _eventRepo.GetNextIdAsync(),
                Title = "Новое событие",
                Description = "Описание",
                StartAt = startAt,
                EndAt = endAt
            };

            await _eventRepo.AddEventAsync(e);
            await _eventRepo.SaveChangesAsync();

            // Читаем через свежий контекст - гарантия, что данные реально ушли в БД
            await using var fresh = _fixture.CreateContext();
            var fromDb = await fresh.Events.FindAsync(e.Id);

            Assert.NotNull(fromDb);
            Assert.Equal("Новое событие", fromDb!.Title);
            Assert.Equal("Описание", fromDb.Description);
            Assert.Equal(startAt, fromDb.StartAt);
            Assert.Equal(endAt, fromDb.EndAt);
            Assert.Equal(50, fromDb.TotalSeats);
            Assert.Equal(50, fromDb.AvailableSeats);
        }

        [Fact]
        public async Task AddEventAsync_WithoutSave_DoesNotPersist()
        {
            var e = new Event(10)
            {
                Id = await _eventRepo.GetNextIdAsync(),
                Title = "Без SaveChanges",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddHours(1)
            };

            await _eventRepo.AddEventAsync(e);
            // НЕ вызываем SaveChanges

            await using var fresh = _fixture.CreateContext();
            Assert.Null(await fresh.Events.FindAsync(e.Id));
        }

        [Fact]
        public async Task UpdateEventByIdAsync_ExistingId_UpdatesFieldsAndPersists()
        {
            var created = await SeedAsync("Старое", DateTime.UtcNow, DateTime.UtcNow.AddHours(1));

            var info = new EventInfo
            {
                Title = "Новое",
                Description = "Обновлено",
                StartAt = new DateTime(2031, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                EndAt = new DateTime(2031, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                TotalSeats = 10
            };

            var updated = await _eventRepo.UpdateEventByIdAsync(created.Id, info);
            await _eventRepo.SaveChangesAsync();

            Assert.NotNull(updated);

            await using var fresh = _fixture.CreateContext();
            var fromDb = await fresh.Events.FindAsync(created.Id);

            Assert.NotNull(fromDb);
            Assert.Equal("Новое", fromDb!.Title);
            Assert.Equal("Обновлено", fromDb.Description);
            Assert.Equal(info.StartAt, fromDb.StartAt);
            Assert.Equal(info.EndAt, fromDb.EndAt);
        }

        [Fact]
        public async Task UpdateEventByIdAsync_MissingId_ReturnsNull()
        {
            var info = new EventInfo
            {
                Title = "x",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddHours(1),
                TotalSeats = 1
            };

            var result = await _eventRepo.UpdateEventByIdAsync(9999, info);
            Assert.Null(result);
        }

        [Fact]
        public async Task RemoveEventByIdAsync_ExistingId_DeletesFromDatabase()
        {
            var created = await SeedAsync("Удалить", DateTime.UtcNow, DateTime.UtcNow.AddHours(1));

            var removed = await _eventRepo.RemoveEventByIdAsync(created.Id);
            await _eventRepo.SaveChangesAsync();

            Assert.NotNull(removed);
            Assert.Equal(created.Id, removed!.Id);

            await using var fresh = _fixture.CreateContext();
            Assert.Null(await fresh.Events.FindAsync(created.Id));
        }

        [Fact]
        public async Task RemoveEventByIdAsync_MissingId_ReturnsNull()
        {
            var result = await _eventRepo.RemoveEventByIdAsync(9999);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetNextIdAsync_EmptyTable_ReturnsOne()
        {
            var id = await _eventRepo.GetNextIdAsync();
            Assert.Equal(1, id);
        }

        [Fact]
        public async Task GetNextIdAsync_NonEmptyTable_ReturnsMaxPlusOne()
        {
            await SeedAsync("A", DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
            await SeedAsync("B", DateTime.UtcNow, DateTime.UtcNow.AddHours(1));

            var id = await _eventRepo.GetNextIdAsync();
            Assert.Equal(3, id);
        }

        [Fact]
        public async Task GetFilteredEvents_NoFilters_ReturnsAll()
        {
            await SeedDatasetAsync();

            var (items, total) = await _eventRepo.GetFilteredEvents(null, null, null, page: 1, pageSize: 10);

            Assert.Equal(5, total);
            Assert.Equal(5, items.Count);
        }

        [Fact]
        public async Task GetFilteredEvents_EmptyTable_ReturnsEmpty()
        {
            var (items, total) = await _eventRepo.GetFilteredEvents(null, null, null, page: 1, pageSize: 10);

            Assert.Equal(0, total);
            Assert.Empty(items);
        }

        [Fact]
        public async Task GetFilteredEvents_TitlePartialCaseInsensitive_ReturnsMatches()
        {
            await SeedDatasetAsync();

            var (items, total) = await _eventRepo.GetFilteredEvents("корол", null, null, 1, 10);

            Assert.Equal(2, total);
            Assert.All(items, e =>
                Assert.Contains("корол", e!.Title!, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetFilteredEvents_TitleUpperCase_StillMatches()
        {
            await SeedDatasetAsync();

            var (items, total) = await _eventRepo.GetFilteredEvents("КОРОЛ", null, null, 1, 10);

            Assert.Equal(2, total);
            Assert.Equal(2, items.Count);
        }

        [Fact]
        public async Task GetFilteredEvents_TitleNoMatch_ReturnsEmpty()
        {
            await SeedDatasetAsync();

            var (items, total) = await _eventRepo.GetFilteredEvents("lolkek", null, null, 1, 10);

            Assert.Equal(0, total);
            Assert.Empty(items);
        }

        [Fact]
        public async Task GetFilteredEvents_EmptyTitle_ReturnsAll()
        {
            await SeedDatasetAsync();

            // Пустая строка — фильтр не применяется (см. !string.IsNullOrEmpty в репо)
            var (items, total) = await _eventRepo.GetFilteredEvents("", null, null, 1, 10);

            Assert.Equal(5, total);
            Assert.Equal(5, items.Count);
        }


        [Fact]
        public async Task GetFilteredEvents_FromOnly_FiltersByStartAt()
        {
            await SeedDatasetAsync();
            var from = new DateTime(2030, 1, 5, 0, 0, 0, DateTimeKind.Utc);

            var (items, total) = await _eventRepo.GetFilteredEvents(null, from, null, 1, 10);

            // Классика (день 10) и Детский (день 20) — после from
            Assert.Equal(2, total);
            Assert.All(items, e => Assert.True(e!.StartAt >= from));
        }

        [Fact]
        public async Task GetFilteredEvents_FromBeforeAll_ReturnsAll()
        {
            await SeedDatasetAsync();
            var from = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var (_, total) = await _eventRepo.GetFilteredEvents(null, from, null, 1, 10);
            Assert.Equal(5, total);
        }

        [Fact]
        public async Task GetFilteredEvents_ToOnly_FiltersByEndAt()
        {
            await SeedDatasetAsync();
            var to = new DateTime(2030, 1, 5, 0, 0, 0, DateTimeKind.Utc);

            var (items, total) = await _eventRepo.GetFilteredEvents(null, null, to, 1, 10);

            // Рок-концерт, Рок-фестиваль, Джаз — все заканчиваются до 5 числа
            Assert.Equal(3, total);
            Assert.All(items, e => Assert.True(e!.EndAt <= to));
        }

        [Fact]
        public async Task GetFilteredEvents_ToAfterAll_ReturnsAll()
        {
            await SeedDatasetAsync();
            var to = new DateTime(2099, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var (_, total) = await _eventRepo.GetFilteredEvents(null, null, to, 1, 10);
            Assert.Equal(5, total);
        }

        [Fact]
        public async Task GetFilteredEvents_TitleAndFromAndTo_AppliesAllFilters()
        {
            await SeedDatasetAsync();
            var from = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = new DateTime(2030, 1, 15, 0, 0, 0, DateTimeKind.Utc);

            var (items, total) = await _eventRepo.GetFilteredEvents("корол", from, to, 1, 10);

            Assert.Equal(2, total);
            Assert.All(items, e =>
            {
                Assert.Contains("корол", e!.Title!, StringComparison.OrdinalIgnoreCase);
                Assert.True(e.StartAt >= from);
                Assert.True(e.EndAt <= to);
            });
        }

        [Fact]
        public async Task GetFilteredEvents_TitleAndFrom_NoOverlap_ReturnsEmpty()
        {
            await SeedDatasetAsync();
            var from = new DateTime(2030, 2, 1, 0, 0, 0, DateTimeKind.Utc);   // позже всех рок-событий

            var (items, total) = await _eventRepo.GetFilteredEvents("рок", from, null, 1, 10);

            Assert.Equal(0, total);
            Assert.Empty(items);
        }

        [Fact]
        public async Task GetFilteredEvents_Page1_ReturnsFirstPageWithCorrectTotal()
        {
            await SeedDatasetAsync();

            var (items, total) = await _eventRepo.GetFilteredEvents(null, null, null, page: 1, pageSize: 2);

            Assert.Equal(5, total);   // total — по всему набору, не по странице
            Assert.Equal(2, items.Count);
        }

        [Fact]
        public async Task GetFilteredEvents_Page2_DoesNotIntersectWithPage1()
        {
            await SeedDatasetAsync();

            var (page1, _) = await _eventRepo.GetFilteredEvents(null, null, null, 1, 2);
            var (page2, _) = await _eventRepo.GetFilteredEvents(null, null, null, 2, 2);

            var ids1 = page1.Select(e => e!.Id).ToHashSet();
            var ids2 = page2.Select(e => e!.Id).ToHashSet();

            Assert.Equal(2, page1.Count);
            Assert.Equal(2, page2.Count);
            Assert.Empty(ids1.Intersect(ids2));
        }

        [Fact]
        public async Task GetFilteredEvents_LastPagePartial_ReturnsRemainder()
        {
            await SeedDatasetAsync();   // 5 элементов

            var (items, total) = await _eventRepo.GetFilteredEvents(null, null, null, page: 3, pageSize: 2);

            Assert.Equal(5, total);
            Assert.Single(items);       // 5 - 2*2 = 1
        }

        [Fact]
        public async Task GetFilteredEvents_PageBeyondEnd_ReturnsEmptyButKeepsTotal()
        {
            await SeedDatasetAsync();

            var (items, total) = await _eventRepo.GetFilteredEvents(null, null, null, page: 10, pageSize: 2);

            Assert.Equal(5, total);
            Assert.Empty(items);
        }

        [Fact]
        public async Task GetFilteredEvents_PageSizeBiggerThanTotal_ReturnsAll()
        {
            await SeedDatasetAsync();

            var (items, total) = await _eventRepo.GetFilteredEvents(null, null, null, page: 1, pageSize: 100);

            Assert.Equal(5, total);
            Assert.Equal(5, items.Count);
        }

        [Fact]
        public async Task GetFilteredEvents_PaginationAppliesToFilteredSet_NotAllRows()
        {
            // title "рок" даёт 2 записи, пагинация по 1: total должен быть 2, а не 5
            await SeedDatasetAsync();

            var (page1, total) = await _eventRepo.GetFilteredEvents("корол", null, null, page: 1, pageSize: 1);
            var (page2, _) = await _eventRepo.GetFilteredEvents("корол", null, null, page: 2, pageSize: 1);

            Assert.Equal(2, total);
            Assert.Single(page1);
            Assert.Single(page2);
            Assert.NotEqual(page1[0]!.Id, page2[0]!.Id);
        }

        [Fact]
        public async Task GetFilteredEvents_PaginationIsStableAcrossCalls()
        {
            // Критично: без OrderBy перед Skip/Take PostgreSQL может вернуть разные строки.
            await SeedDatasetAsync();

            var (page1a, _) = await _eventRepo.GetFilteredEvents(null, null, null, 1, 3);
            var (page1b, _) = await _eventRepo.GetFilteredEvents(null, null, null, 1, 3);

            Assert.Equal(
                page1a.Select(e => e!.Id).ToArray(),
                page1b.Select(e => e!.Id).ToArray());
        }

        [Fact]
        public async Task ReleaseSeats_ThenSave_AvailableSeatsPersistedInDatabase()
        {
            // Ловит случай, когда _availableSeats не смаплен как backing field —
            // тогда EF сохранит только TotalSeats, а AvailableSeats не обновится.
            var created = await SeedAsync("Места", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), totalSeats: 5);

            var loaded = await _eventRepo.GetEventByIdAsync(created.Id);
            Assert.Equal(5, loaded!.AvailableSeats);

            loaded.ReleaseSeats(2);   // 5 → 7
            await _eventRepo.SaveChangesAsync();

            // Свежий контекст — читаем из БД, а не из трекера
            await using var fresh = _fixture.CreateContext();
            var fromDb = await fresh.Events.FindAsync(created.Id);

            Assert.NotNull(fromDb);
            Assert.Equal(7, fromDb!.AvailableSeats);
        }

        [Fact]
        public async Task TryReserveSeats_ThenSave_AvailableSeatsDecreasedInDatabase()
        {
            var created = await SeedAsync("Резерв", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), totalSeats: 3);

            var loaded = await _eventRepo.GetEventByIdAsync(created.Id);
            Assert.True(loaded!.TryReserveSeats(2));   // 3 → 1

            await _eventRepo.SaveChangesAsync();

            await using var fresh = _fixture.CreateContext();
            var fromDb = await fresh.Events.FindAsync(created.Id);
            Assert.Equal(1, fromDb!.AvailableSeats);
        }

        private async Task<Event> SeedAsync(string title, DateTime startAt, DateTime endAt, int totalSeats = 10)
        {
            var e = new Event(totalSeats)
            {
                Id = await _eventRepo.GetNextIdAsync(),
                Title = title,
                StartAt = startAt,
                EndAt = endAt
            };
            await _eventRepo.AddEventAsync(e);
            await _eventRepo.SaveChangesAsync();
            return e;
        }

        // Набор из 5 событий
        private async Task SeedDatasetAsync()
        {
            var baseDate = new DateTime(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            await SeedAsync("Белоснежка", baseDate.AddDays(1), baseDate.AddDays(1).AddHours(3));
            await SeedAsync("Король и шут", baseDate.AddDays(2), baseDate.AddDays(2).AddHours(6));
            await SeedAsync("Сплин", baseDate.AddDays(3), baseDate.AddDays(3).AddHours(2));
            await SeedAsync("В пещере горного короля", baseDate.AddDays(10), baseDate.AddDays(10).AddHours(2));
            await SeedAsync("Rammstein", baseDate.AddDays(20), baseDate.AddDays(20).AddHours(1));
        }
    }
}
