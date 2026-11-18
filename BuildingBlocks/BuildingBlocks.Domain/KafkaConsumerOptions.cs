using System;
using System.Collections.Generic;
using System.Text;

namespace BuildingBlocks.Domain
{
    public class KafkaConsumerOptions
    {
        public const string SectionName = "Kafka";
        public string? BootstrapServers { get; set; }
        public string? ConsumerGroup {  get; set; }
    }
}
