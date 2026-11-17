using BookingService.Application.Interfaces;
using BuildingBlocks.Domain;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BookingService.Infrastructure.Kafka
{
    public class BookingPublisher : IBookingPublisher, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<BookingPublisher> _logger;

        public BookingPublisher(IOptions<KafkaOptions> options,
                                ILogger<BookingPublisher> logger)
        {
            _logger = logger;
            var o = options.Value;

            var config = new ProducerConfig
            {
                BootstrapServers = o.BootstrapServers,
                Acks = Enum.Parse<Acks>(o.Acks, ignoreCase: true)
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        // Создание продюсера, сериализаия события в JSON и отпрака в топик
        public async Task PublishAsync<T>(string topic, string key, T bookingResponse, CancellationToken ct = default) where T : class
        {
            try
            {
                var result = await _producer.ProduceAsync(topic, new Message<string, string>
                {
                    Key = key,
                    Value = JsonSerializer.Serialize(bookingResponse)
                }, ct);
                _logger.LogInformation($"Доставлено {typeof(T).Name} в топик {topic}, партиция {result.Partition.Value}, Оффсет {result.Offset.Value}, ключ {key}");
            }
            catch(ProduceException<string,string> ex)
            {
                _logger.LogError(ex, $"Ошибка доставки {typeof(T).Name} в топик {topic}, ключ {key}: {ex.Error.Reason}");
            }
        }
        public void Dispose()
        {
            _producer.Flush(TimeSpan.FromSeconds(10));
            _producer.Dispose();
        }
    }
}
