using System;
using System.Threading.Tasks;

namespace SplitMoney.Client.Services
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, bool persist = false);
        Task RemoveAsync(string key);
        Task ClearAllAsync();
    }
}
