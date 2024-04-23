using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace GroceryStore.Persistance.Repositories.Cache;
public static class DistributedCacheExt
{
    public static async Task SetRecordAsync<T>(this IDistributedCache cache,
        string key,
        T value,
        TimeSpan? absoluteExpirationTime = null,
        TimeSpan? slidingExpirationTime = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpirationTime ?? TimeSpan.FromMinutes(1),
            SlidingExpiration = slidingExpirationTime
        };
        var jsonValue = JsonSerializer.Serialize(value);
        await cache.SetStringAsync(key, jsonValue, options);
    }
    public static async Task<T?> GetRecordAsync<T>(this IDistributedCache cache, string key)
    {
        var jsonValue = await cache.GetStringAsync(key);
        if (jsonValue is null)
        {
            return default;
        }
        return JsonSerializer.Deserialize<T>(jsonValue);
    }
}
