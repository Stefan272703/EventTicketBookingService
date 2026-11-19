using BuildingBlocks.Domain;
using Confluent.Kafka;
using EventService.Application.Repository.Interface;
using EventTicketBooking.Contracts.Bookings;
using EventTicketBooking.Contracts.Topics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EventService.Infrastructure.Workers
{
    public class EventConsumerWorker : BackgroundService
    {
        private readonly ILogger<EventConsumerWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConsumer<string, string> _consumer;
        public EventConsumerWorker(IOptions<KafkaConsumerOptions> options,
                                   IServiceScopeFactory ScopeFactory,
                                   ILogger<EventConsumerWorker> logger)
        {
            _logger = logger;
            _scopeFactory = ScopeFactory;
            var o = options.Value;

            var config = new ConsumerConfig
            {
                BootstrapServers = o.BootstrapServers,
                GroupId = o.ConsumerGroup,
                AutoOffsetReset = Enum.Parse<AutoOffsetReset>(o.AutoOffsetReset, ignoreCase: true),
                EnableAutoCommit = o.EnableAutoCommit,
                EnableAutoOffsetStore = o.EnableAutoOffsetStore,
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => Consume(stoppingToken));
        }

        private async Task Consume(CancellationToken stoppingToken)
        {
            // Подписываемся на топик booking-confirmed
            _consumer.Subscribe(BookingTopics.BookingConfirmed);
            _logger.LogInformation($"Consumer запущен. Ожидание сообщений из топика {BookingTopics.BookingConfirmed}...");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    ConsumeResult<string, string>? consumeResult = null;
                    try
                    {
                        // Блокирующий вызов: ждём следующего сообщения от брокера (pull-модель)
                        // При отмене CancellationToken бросает OperationCanceledException
                        consumeResult = _consumer.Consume(stoppingToken);

                        var @event = JsonSerializer.Deserialize<BookingConfirmedIntegrationEvent>(consumeResult.Message.Value);

                        if (@event == null)
                        {
                            _logger.LogWarning($"Не удалось десериализовать сообщение offset = {consumeResult.Offset}");
                            _consumer.Commit(consumeResult);

                            continue;
                        }

                        // Scope с репозиторием Event
                        using var scope = _scopeFactory.CreateScope();
                        var eventRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();

                        // Сущность Event
                        var eventEntity = await eventRepo.GetEventByIdAsync(@event.EventId, stoppingToken);
                        // Если данной сущности не существует
                        if (eventEntity == null)
                        {
                            _logger.LogWarning($"BookingConfirmed {@event.BookingId}: событие {@event.EventId} не найдено");

                            _consumer.Commit(consumeResult);
                            continue;
                        }

                        // Если не получилось зарезервировать место
                        if (!eventEntity.TryReserveSeats(@event.SeatsCount))
                        {
                            _logger.LogWarning($"BookingConfirmed {@event.BookingId}: событие {@event.EventId} не имеет {@event.SeatsCount} свободных мест");

                            _consumer.Commit(consumeResult);
                            continue;
                        }

                        await eventRepo.SaveChangesAsync(stoppingToken);
                        _consumer.Commit(consumeResult);

                        _logger.LogInformation($"BookingConfirmed {@event.BookingId}: событие {eventEntity.Id} уменьшил AvailableSeats на {@event.SeatsCount} свободных мест");
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError($"Ошибка consume: {ex.Message}");
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Ошибка обработки сообщения OffSet {consumeResult?.Offset}");
                        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    }
                }
            }
            finally
            {
                _consumer.Close();
                _logger.LogInformation("Kafka consumer остановлен.");
            }
        }
    }
}
