using System;
using System.Collections.Generic;
using System.Text;

namespace EventService.Application.Interfaces.Redis
{
    public interface ICacheService
    {
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
        public Task SetAsync<T>(string key, T value, TimeSpan? TTL = null, CancellationToken cancellationToken = default) where T : class;
        public Task RemoveKeyAsync(string key, CancellationToken cancellationToken = default);
    }
}
