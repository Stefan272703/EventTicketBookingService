namespace BuildingBlocks.Domain
{
    public class KafkaProducerOptions
    {
        public const string SectionName = "Kafka";
        public string? BootstrapServers { get; set; }
        public string? Acks { get; set; }
    }
}
