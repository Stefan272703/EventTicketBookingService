using BookingService.Infrastructure.DataAccess;
using BookingService.Domain.Models;
using BookingService.Infrastructure.Repository;

namespace EventTicketBookingService.IntegrationTests
{
    [Collection(PostgresCollection.Name)]
    public class BookingRepositoryTests : IAsyncLifetime
    {
        private readonly PostgresContainerFixture _fixture;
        private AppDbContext _context;
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
            _bookingRepo = new BookingRepository(_context);
        }

        [Fact]
        public async Task GetBookingByIdAsync_ExistingId_ReturnsBooking()
        {
            var b = await SeedBookingAsync();

            var loaded = await _bookingRepo.GetBookingByIdAsync(b.Id);

            Assert.NotNull(loaded);
            Assert.Equal(b.Id, loaded!.Id);
            Assert.Equal(b.EventId, loaded.EventId);
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
            var createdAt = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var b = Booking.Create(eventId: 42, userId: 7, createdAt);

            await _bookingRepo.AddBookingAsync(b);
            await _bookingRepo.SaveChangesAsync();

            await using var fresh = _fixture.CreateContext();
            var fromDb = await fresh.Bookings.FindAsync(b.Id);

            Assert.NotNull(fromDb);
            Assert.Equal(b.UserId, fromDb!.UserId);
            Assert.Equal(b.EventId, fromDb!.EventId);
            Assert.Equal(BookingStatus.Pending, fromDb.Status);
            Assert.Equal(createdAt, fromDb.CreatedAt);
            Assert.Null(fromDb.ProcessedAt);
        }

        [Fact]
        public async Task AddBookingAsync_WithoutSave_DoesNotPersist()
        {
            var b = Booking.Create(eventId: 42, userId: 7, DateTime.UtcNow);

            await _bookingRepo.AddBookingAsync(b);
            // НЕ вызываем SaveChanges

            await using var fresh = _fixture.CreateContext();
            Assert.Null(await fresh.Bookings.FindAsync(b.Id));
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
            //var e = await SeedEventAsync();
            var p1 = await SeedBookingAsync(BookingStatus.Pending);
            var p2 = await SeedBookingAsync(BookingStatus.Pending);
            await SeedBookingAsync(BookingStatus.Confirmed);
            await SeedBookingAsync(BookingStatus.Cancelled);
            await SeedBookingAsync(BookingStatus.Rejected);

            var ids = await _bookingRepo.GetBookingPendingIdsAsync(CancellationToken.None);

            Assert.Equal(2, ids.Count);
            Assert.Contains(p1.Id, ids);
            Assert.Contains(p2.Id, ids);
        }

        [Fact]
        public async Task GetBookingPendingIdsAsync_AfterConfirm_ExcludesIt()
        {
            var b = await SeedBookingAsync();

            var loaded = await _bookingRepo.GetBookingByIdAsync(b.Id);
            loaded!.Confirm();
            await _bookingRepo.SaveChangesAsync();

            var ids = await _bookingRepo.GetBookingPendingIdsAsync(CancellationToken.None);
            Assert.DoesNotContain(b.Id, ids);
        }

        [Fact]
        public async Task GetBookingPendingIdsAsync_AfterReject_ExcludesIt()
        {
            var b = await SeedBookingAsync();

            var loaded = await _bookingRepo.GetBookingByIdAsync(b.Id);
            loaded!.Reject();
            await _bookingRepo.SaveChangesAsync();

            var ids = await _bookingRepo.GetBookingPendingIdsAsync(CancellationToken.None);
            Assert.DoesNotContain(b.Id, ids);
        }

        [Fact]
        public async Task Confirm_ThenSave_PersistsConfirmedStatusAndProcessedAt()
        {
            var b = await SeedBookingAsync();

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
            var b = await SeedBookingAsync();

            var loaded = await _bookingRepo.GetBookingByIdAsync(b.Id);
            loaded!.Reject();
            await _bookingRepo.SaveChangesAsync();

            await using var fresh = _fixture.CreateContext();
            var fromDb = await fresh.Bookings.FindAsync(b.Id);

            Assert.Equal(BookingStatus.Rejected, fromDb!.Status);
            Assert.NotNull(fromDb.ProcessedAt);
        }

        private async Task<Booking> SeedBookingAsync(BookingStatus status = BookingStatus.Pending, int eventId = 1, int userId = 1)
        {
            var b = Booking.Create(eventId, userId, DateTime.UtcNow);
            if (status == BookingStatus.Confirmed) b.Confirm();
            else if (status == BookingStatus.Cancelled) b.Cancel();
            else if (status == BookingStatus.Rejected) b.Reject();

            await _bookingRepo.AddBookingAsync(b);
            await _bookingRepo.SaveChangesAsync();
            return b;
        }

    }
}
