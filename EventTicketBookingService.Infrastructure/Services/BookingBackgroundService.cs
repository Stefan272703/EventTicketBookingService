using EventTicketBookingService.Domain.Models;
using EventTicketBookingService.Application.Repository.Interface;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace EventTicketBookingService.Infrastructure.Services
{
    public class BookingBackgroundService : BackgroundService
    {
        private readonly SemaphoreSlim _processingSemaphore = new(1, Environment.ProcessorCount);
        private readonly ILogger<BookingBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        // Задержки времени от и до для случайного времени внешнего вызова(выраженное в мс)
        private readonly int minDelay = 1000;
        private readonly int maxDelay = 5000;

        public BookingBackgroundService(ILogger<BookingBackgroundService> logger,
            IServiceScopeFactory ScopeFactory
            )
        {
            _logger = logger;
            _scopeFactory = ScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Фоновый сервис брони запущен.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Имитация внешнего вызова
                    Random random = new Random();
                    int delayTime = random.Next(minDelay, maxDelay + 1);
                    await Task.Delay(delayTime, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                await _processingSemaphore.WaitAsync(stoppingToken);

                try
                {
                    List<int> pendingBookingsIds;

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var bookingRepo = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                        pendingBookingsIds = await bookingRepo.GetBookingPendingIdsAsync(stoppingToken);
                    }
                    var tasks = pendingBookingsIds.Select(bookingId => ProcessBookingAsync(bookingId, stoppingToken));

                    await Task.WhenAll(tasks);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ошибка при получении данных о брони");
                }
                finally
                {
                    _processingSemaphore.Release();
                }
            }
            _logger.LogInformation("Фоновый сервис брони остановлен.");
        }

        private async Task<Booking> ProcessBookingAsync(int bookingId, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Проходит процесс над бронью с ID: {bookingId}. Подождите пару секунд.");
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var eventRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();
                var bookingRepo = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                var booking = await bookingRepo.GetBookingByIdAsync(bookingId, stoppingToken);
                if (booking == null || booking.Status != BookingStatus.Pending)
                {
                    return null;
                }

                var @event = await eventRepo.GetEventByIdAsync(booking.EventId, stoppingToken);

                if (@event != null)
                {
                    booking.Confirm();
                    await bookingRepo.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation($"Процесс над бронью с ID: {booking.Id} завершен успешно!");
                    return booking;
                }
                else
                {
                    booking.Reject();
                    await bookingRepo.SaveChangesAsync(stoppingToken);
                    _logger.LogWarning($"Не обработана бронь с ID {booking.Id} из-за отсутствия события по ID: {booking.EventId}.");
                    return booking;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogWarning($"Обработка брони ID: {bookingId} прервана из-за отмены");

                using var scope = _scopeFactory.CreateScope();
                var eventRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();
                var bookingRepo = scope.ServiceProvider.GetRequiredService<IBookingRepository>();


                var booking = await bookingRepo.GetBookingByIdAsync(bookingId, stoppingToken);

                var @event = await eventRepo.GetEventByIdAsync(booking.EventId, stoppingToken);
                if (@event != null)
                {
                    booking.Reject();
                    @event?.ReleaseSeats();
                    await bookingRepo.SaveChangesAsync(stoppingToken);
                }

                throw;
            }
            catch (Exception)
            {
                using var scope = _scopeFactory.CreateScope();

                var eventRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();
                var bookingRepo = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                var booking = await bookingRepo.GetBookingByIdAsync(bookingId, stoppingToken);

                var @event = await eventRepo.GetEventByIdAsync(booking.EventId, stoppingToken);

                if (@event != null)
                {
                    booking.Reject();
                    @event?.ReleaseSeats();
                    await bookingRepo.SaveChangesAsync(stoppingToken);
                    _logger.LogError($"Непредвиденная ошибка обработки брони, {booking.EventId}. Вовзращаем место.");
                }
                return booking;
            }
        }
    }
}
