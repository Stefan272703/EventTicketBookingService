using System;
using System.Collections.Generic;
using System.Text;

namespace EventService.Application
{
    public static class CacheKeys
    {
        public static string Event(int id) => $"event:{id}";
        public static string Top10Events = "events:top10";
    }
}
