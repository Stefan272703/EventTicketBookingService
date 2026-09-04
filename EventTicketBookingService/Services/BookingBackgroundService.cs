using EventTicketBookingService.Exceptions;
using EventTicketBookingService.Interfaces;
using EventTicketBookingService.Models;
using System.Net.NetworkInformation;

namespace EventTicketBookingService.Services
{
    public class BookingBackgroundService: BackgroundService
    {
        private readonly SemaphoreSlim _processingSemaphore = new(1, 1);
        private readonly ILogger<BookingBackgroundService> _logger;
        private readonly IBookingTaskQueue _bookingStore;
        private readonly IBookingService _bookingService;
        private readonly IEventStore _eventStore; 
        // Заедержки времени от и до для случайной времени внешнего вызова(выраженное в мс)
        private readonly int minDelay = 1000; 
        private readonly int maxDelay = 5000;

        public BookingBackgroundService(ILogger<BookingBackgroundService> logger,
                                        IBookingTaskQueue taskQueue,
                                        IBookingService bookingService,
                                        IEventStore eventStore)
        {
            _logger = logger;
            _bookingStore = taskQueue;
            _bookingService = bookingService;
            _eventStore = eventStore;
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

                    await _processingSemaphore.WaitAsync();

                    var pendingBookings = _bookingStore.GetPending().ToList();
                    var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, stoppingToken));
                    
                    await Task.WhenAll(tasks);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) 
                {
                    break;
                }
                catch(Exception ex)
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

        private async Task<Booking> ProcessBookingAsync(Booking booking, CancellationToken stoppingToken)
        {
            if (_bookingStore.TryDequeue(out var task) && task?.Status == BookingStatus.Pending)
            {             
                _logger.LogInformation($"Проходит процесс над бронью с ID: {task.Id}. Подождите пару секунд.");
                try
                {
                    if (_eventStore.TryGetEventById(task.EventId, out var @event))
                    {
                        booking.Confirm();
                        _bookingStore.Update(booking);
                        await _bookingService.UpdateBookingStatusAsync(task.Id, booking.Status, stoppingToken);
                        _logger.LogInformation($"Процесс над бронью с ID: {task.Id} завершен успешно!");
                        return booking;
                    }
                    else
                    {
                        booking.Reject();
                        _bookingStore.Update(booking);
                        await _bookingService.UpdateBookingStatusAsync(task.Id, booking.Status, stoppingToken);
                        _logger.LogWarning($"Не обработана бронь с ID {task.Id} из-за отсуствия события по ID: {task.EventId}.");
                        return booking;
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning($"Обработка брони ID: {task.Id} прервана из-за отмены");
                    if(_eventStore.TryGetEventById(task.EventId, out var @event))
                    {
                        booking.Reject();
                        @event?.ReleaseSeats();
                        _bookingStore.Update(booking);
                        await _bookingService.UpdateBookingStatusAsync(task.Id, booking.Status, stoppingToken);
                    }
                    
                    throw;
                }
                catch (Exception)
                {
                    if (_eventStore.TryGetEventById(task.EventId, out var @event))
                    {
                        booking.Reject();
                        @event?.ReleaseSeats();
                        _bookingStore.Update(booking);
                        await _bookingService.UpdateBookingStatusAsync(task.Id, booking.Status, stoppingToken);
                        _logger.LogError($"Непредвиденная ошибка обработки брони, {task.EventId}. Вовзращаем место.");
                    }
                }
            }
            return booking;
        }
    }
}
