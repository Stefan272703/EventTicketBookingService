using System;
using System.Collections.Generic;
using System.Text;

namespace BuildingBlocks.Domain
{
    public class RedisOptions
    {
        public const string SectionName = "Redis";
        public string? ConnectionString { get; set; }
        public int TTL { get; set; }
    }
}
