namespace BuildingBlocks.Domain
{
    public class KafkaOptions
    {
        public const string SectionName = "Kafka";
        public string? BootstrapServers { get; set; }
        public string? Acks { get; set; }
    }
}
