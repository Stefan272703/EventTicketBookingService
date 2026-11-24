using BuildingBlocks.Domain;
using EventService.Application.Interfaces.Redis;
using EventService.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace EventService.Infrastructure.Redis
{
    public class CacheService : ICacheService
    {
        private readonly ILogger<CacheService> _logger;
        private readonly IDatabase _database;
        private readonly RedisOptions _redisOptions;

        public CacheService(IConnectionMultiplexer connection,
                            ILogger<CacheService> logger,
                            IOptions<RedisOptions> redisOptions)
        {
            // Объект для работы с базами данных
            _logger = logger;

            IDatabase database = connection.GetDatabase();
            _database = database;
            _redisOptions = redisOptions.Value;
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                await _database.PingAsync();

                var value = await _database.StringGetAsync(key);
                if (!value.HasValue)
                    return null;
                
                return JsonSerializer.Deserialize<T>(value.ToString());
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError($"Не удалось подключиться к Redis {ex.Message}");
                return null;
            }
            catch(RedisException ex)
            {
                _logger.LogWarning($"Не получилось получить кеш по ключу {key}.");

                return null;
            }
        }

        public async Task RemoveKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _database.PingAsync();
                
                // Удаление кеша по ключу
                await _database.KeyDeleteAsync(key);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError($"Не удалось подключиться к Redis {ex.Message}");
            }
            catch (RedisException ex)
            {
                _logger.LogWarning($"Не получилось удалить кеш по ключу {key}.");

            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? TTL = null, CancellationToken cancellationToken = default) where T : class
        {
            string json = JsonSerializer.Serialize(value);
            try
            {
                await _database.PingAsync();

                // Время жизни в кеше
                var ttl = TTL ?? TimeSpan.FromMinutes(_redisOptions.TTL);

                await _database.StringSetAsync(key, json, ttl);
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError($"Не удалось подключиться к Redis {ex.Message}");
            }
            catch (RedisException ex)
            {
                _logger.LogWarning($"Не получилось записать в кеш по ключу {key} со значением {json}.");
            }
        }
    }
}
