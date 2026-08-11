using EventTicketBookingService.Interfaces;

namespace EventTicketBookingService.Services
{
    public class BookingBackgroundService: BackgroundService
    {
        private readonly ILogger<BookingBackgroundService> _logger;
        private readonly IBookingTaskQueue _taskQueue;

        public BookingBackgroundService(ILogger<BookingBackgroundService> logger,
                                        IBookingTaskQueue taskQueue)
        {
            _logger = logger;
            _taskQueue = taskQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Фоновый сервис брони запущен.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_taskQueue.TryDequeue(out var task) && task.Status == Models.BookingStatus.Pending) 
                    {
                        _logger.LogInformation($"Проходит процесс над бронью с ID: {task.Id}. Подождить пару секунд.");
                        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

                        task.ProcessedAt = DateTime.Now;
                        task.Status = Models.BookingStatus.Confirmed;

                        _logger.LogInformation($"Процесс над бронью с ID: {task.Id} завершен!");

                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) 
                {
                    break;
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "ошибка при получении данных о брони");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
                catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
            _logger.LogInformation("Фоновый сервис брони остановлен.");
        }

    }
}
