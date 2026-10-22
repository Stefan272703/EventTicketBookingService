using EventTicketBookingService.DataAccess;
using EventTicketBookingService.Models;
using EventTicketBookingService.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventTicketBookingService.IntegrationTests
{
    [Collection(PostgresCollection.Name)]
    public class BookingRepositoryTests : IAsyncLifetime
    {
        private readonly PostgresContainerFixture _fixture;
        private AppDbContext _context;
        private EventRepository _eventRepo;
        private BookingRepository _bookingRepo;

        public BookingRepositoryTests(PostgresContainerFixture fixture)
        {
            _fixture = fixture;
        }

        public async Task DisposeAsync() => await _context.DisposeAsync();

        public async Task InitializeAsync()
        {
            await _fixture.ResetDatabaseAsync();
            _context = _fixture.CreateContext();
            _eventRepo = new EventRepository(_context);
            _bookingRepo = new BookingRepository(_context);
        }

        [Fact]
        public async Task GetBookingByIdAsync_ExistingId_ReturnsBooking()
        {
            var e = await SeedEventAsync();
            var b = await SeedBookingAsync(e.Id);

            var loaded = await _bookingRepo.GetBookingByIdAsync(b.Id);

            Assert.NotNull(loaded);
            Assert.Equal(b.Id, loaded!.Id);
            Assert.Equal(e.Id, loaded.EventId);
            Assert.Equal(BookingStatus.Pending, loaded.Status);
            Assert.Null(loaded.ProcessedAt);
        }

        [Fact]
        public async Task GetBookingByIdAsync_MissingId_ReturnsNull()
        {
            var result = await _bookingRepo.GetBookingByIdAsync(9999);
            Assert.Null(result);
        }

        [Fact]
        public async Task AddBookingAsync_ThenSave_PersistsAllFields()
        {
            var e = await SeedEventAsync();
            var createdAt = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var b = new Booking
            {
                Id = await _bookingRepo.GetNextIdAsync(),
                EventId = e.Id,
                Status = BookingStatus.Pending,
                CreatedAt = createdAt
            };

            await _bookingRepo.AddBookingAsync(b);
            await _bookingRepo.SaveChangesAsync();

            await using var fresh = _fixture.CreateContext();
            var fromDb = await fresh.Bookings.FindAsync(b.Id);

            Assert.NotNull(fromDb);
            Assert.Equal(e.Id, fromDb!.EventId);
            Assert.Equal(BookingStatus.Pending, fromDb.Status);
            Assert.Equal(createdAt, fromDb.CreatedAt);
            Assert.Null(fromDb.ProcessedAt);
        }

        [Fact]
        public async Task AddBookingAsync_WithoutSave_DoesNotPersist()
        {
            var e = await SeedEventAsync();
            var b = new Booking
            {
                Id = await _bookingRepo.GetNextIdAsync(),
                EventId = e.Id,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _bookingRepo.AddBookingAsync(b);
            // НЕ вызываем SaveChanges

            await using var fresh = _fixture.CreateContext();
            Assert.Null(await fresh.Bookings.FindAsync(b.Id));
        }

        [Fact]
        public async Task AddBookingAsync_NonExistingEventId_ThrowsOnSave()
        {
            // Проверяет, что FK Booking.EventId → Event.Id существует в БД.
            // Если FK нет — тест упадёт, потому что SaveChanges пройдёт без ошибки.
            var b = new Booking
            {
                Id = await _bookingRepo.GetNextIdAsync(),
                EventId = 9999,                 // такого события нет
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            await _bookingRepo.AddBookingAsync(b);

            await Assert.ThrowsAsync<DbUpdateException>(() => _bookingRepo.SaveChangesAsync());
        }

        [Fact]
        public async Task GetNextIdAsync_Empty_ReturnsOne()
        {
            Assert.Equal(1, await _bookingRepo.GetNextIdAsync());
        }

        [Fact]
        public async Task GetNextIdAsync_NonEmpty_ReturnsMaxPlusOne()
        {
            var e = await SeedEventAsync();
            await SeedBookingAsync(e.Id);
            await SeedBookingAsync(e.Id);

            Assert.Equal(3, await _bookingRepo.GetNextIdAsync());
        }

        [Fact]
        public async Task GetBookingPendingIdsAsync_EmptyTable_ReturnsEmpty()
        {
            var ids = await _bookingRepo.GetBookingPendingIdsAsync(CancellationToken.None);
            Assert.Empty(ids);
        }

        [Fact]
        public async Task GetBookingPendingIdsAsync_ReturnsOnlyPending()
        {
            var e = await SeedEventAsync();
            var p1 = await SeedBookingAsync(e.Id, BookingStatus.Pending);
            var p2 = await SeedBookingAsync(e.Id, BookingStatus.Pending);
            await SeedBookingAsync(e.Id, BookingStatus.Confirmed);
            await SeedBookingAsync(e.Id, BookingStatus.Rejected);

            var ids = await _bookingRepo.GetBookingPendingIdsAsync(CancellationToken.None);

            Assert.Equal(2, ids.Count);
            Assert.Contains(p1.Id, ids);
            Assert.Contains(p2.Id, ids);
        }

        [Fact]
        public async Task GetBookingPendingIdsAsync_AfterConfirm_ExcludesIt()
        {
            var e = await SeedEventAsync();
            var b = await SeedBookingAsync(e.Id);

            var loaded = await _bookingRepo.GetBookingByIdAsync(b.Id);
            loaded!.Confirm();
            await _bookingRepo.SaveChangesAsync();

            var ids = await _bookingRepo.GetBookingPendingIdsAsync(CancellationToken.None);
            Assert.DoesNotContain(b.Id, ids);
        }

        [Fact]
        public async Task GetBookingPendingIdsAsync_AfterReject_ExcludesIt()
        {
            var e = await SeedEventAsync();
            var b = await SeedBookingAsync(e.Id);

            var loaded = await _bookingRepo.GetBookingByIdAsync(b.Id);
            loaded!.Reject();
            await _bookingRepo.SaveChangesAsync();

            var ids = await _bookingRepo.GetBookingPendingIdsAsync(CancellationToken.None);
            Assert.DoesNotContain(b.Id, ids);
        }

        [Fact]
        public async Task Confirm_ThenSave_PersistsConfirmedStatusAndProcessedAt()
        {
            var e = await SeedEventAsync();
            var b = await SeedBookingAsync(e.Id);

            var loaded = await _bookingRepo.GetBookingByIdAsync(b.Id);
            loaded!.Confirm();
            await _bookingRepo.SaveChangesAsync();

            await using var fresh = _fixture.CreateContext();
            var fromDb = await fresh.Bookings.FindAsync(b.Id);

            Assert.Equal(BookingStatus.Confirmed, fromDb!.Status);
            Assert.NotNull(fromDb.ProcessedAt);
        }

        [Fact]
        public async Task Reject_ThenSave_PersistsRejectedStatusAndProcessedAt()
        {
            var e = await SeedEventAsync();
            var b = await SeedBookingAsync(e.Id);

            var loaded = await _bookingRepo.GetBookingByIdAsync(b.Id);
            loaded!.Reject();
            await _bookingRepo.SaveChangesAsync();

            await using var fresh = _fixture.CreateContext();
            var fromDb = await fresh.Bookings.FindAsync(b.Id);

            Assert.Equal(BookingStatus.Rejected, fromDb!.Status);
            Assert.NotNull(fromDb.ProcessedAt);
        }

        private async Task<Event> SeedEventAsync(int totalSeats = 10)
        {
            var e = new Event(totalSeats)
            {
                Id = await _eventRepo.GetNextIdAsync(),
                Title = "Событие",
                Description = "Описание",
                StartAt = DateTime.UtcNow,
                EndAt = DateTime.UtcNow.AddHours(1)
            };
            await _eventRepo.AddEventAsync(e);
            await _eventRepo.SaveChangesAsync();
            return e;
        }

        [Fact]
        public async Task RemoveEvent_CascadesDeleteBookings_AtDatabaseLevel()
        {
            // Arrange: событие + 2 брони
            var e = await SeedEventAsync();
            var b1 = await SeedBookingAsync(e.Id, BookingStatus.Pending);
            var b2 = await SeedBookingAsync(e.Id, BookingStatus.Confirmed);

            // Act: удаляем событие через СВЕЖИЙ контекст, где Bookings НЕ загружены.
            // Значит, EF не может каскадировать на уровне трекера —
            // только БД (ON DELETE CASCADE) может это сделать.
            await using (var freshContext = _fixture.CreateContext())
            {
                var freshEventRepo = new EventRepository(freshContext);
                var removed = await freshEventRepo.RemoveEventByIdAsync(e.Id);
                await freshEventRepo.SaveChangesAsync();

                Assert.NotNull(removed);
            }

            // Assert: событие и обе брони исчезли из БД
            await using var verifyContext = _fixture.CreateContext();
            Assert.Null(await verifyContext.Events.FindAsync(e.Id));
            Assert.Null(await verifyContext.Bookings.FindAsync(b1.Id));
            Assert.Null(await verifyContext.Bookings.FindAsync(b2.Id));
        }

        [Fact]
        public async Task RemoveEvent_CascadesDeleteBookings_DoesNotTouchOtherEventsBookings()
        {
            // Arrange: два события, у каждого свои брони
            var e1 = await SeedEventAsync();
            var e2 = await SeedEventAsync();
            var b1 = await SeedBookingAsync(e1.Id);
            var b2 = await SeedBookingAsync(e2.Id);

            // Act: удаляем только e1
            await using (var freshContext = _fixture.CreateContext())
            {
                var freshEventRepo = new EventRepository(freshContext);
                await freshEventRepo.RemoveEventByIdAsync(e1.Id);
                await freshEventRepo.SaveChangesAsync();
            }

            // Assert: b1 удалена (каскад от e1), b2 жива (e2 не трогали)
            await using var verifyContext = _fixture.CreateContext();
            Assert.Null(await verifyContext.Bookings.FindAsync(b1.Id));
            Assert.NotNull(await verifyContext.Bookings.FindAsync(b2.Id));
        }

        [Fact]
        public async Task RemoveEvent_ThenGetPendingIds_ReturnsRemainingPendingOnly()
        {
            // Arrange
            var e1 = await SeedEventAsync();
            var e2 = await SeedEventAsync();
            var b1 = await SeedBookingAsync(e1.Id, BookingStatus.Pending);
            var b2 = await SeedBookingAsync(e2.Id, BookingStatus.Pending);

            // Act: удаляем e1 со всеми его pending-бронями
            await using (var freshContext = _fixture.CreateContext())
            {
                var freshEventRepo = new EventRepository(freshContext);
                await freshEventRepo.RemoveEventByIdAsync(e1.Id);
                await freshEventRepo.SaveChangesAsync();
            }

            // Assert
            await using var verifyContext = _fixture.CreateContext();
            var pendingRepo = new BookingRepository(verifyContext);
            var ids = await pendingRepo.GetBookingPendingIdsAsync(CancellationToken.None);

            Assert.Single(ids);
            Assert.Contains(b2.Id, ids);
            Assert.DoesNotContain(b1.Id, ids);
        }

        private async Task<Booking> SeedBookingAsync(int eventId,
                                                     BookingStatus status = BookingStatus.Pending)
        {
            var b = new Booking
            {
                Id = await _bookingRepo.GetNextIdAsync(),
                EventId = eventId,
                Status = status,
                CreatedAt = DateTime.UtcNow
            };
            await _bookingRepo.AddBookingAsync(b);
            await _bookingRepo.SaveChangesAsync();
            return b;
        }

    }
}
