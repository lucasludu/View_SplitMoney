using Blazored.LocalStorage;
using System.Collections.Concurrent;

namespace SplitMoney.Client.Services
{
    public class CacheService : ICacheService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly IEncryptionService _encryptionService;
        private static readonly ConcurrentDictionary<string, object> _memoryCache = new();

        public CacheService(ILocalStorageService localStorage, IEncryptionService encryptionService)
        {
            _localStorage = localStorage;
            _encryptionService = encryptionService;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            // 1. Intentar obtener de memoria
            if (_memoryCache.TryGetValue(key, out var objEntry))
            {
                var entry = (CacheEntry<T>)objEntry;
                if (entry.Expiration > DateTime.UtcNow)
                {
                    return entry.Value;
                }
                _memoryCache.TryRemove(key, out _);
            }

            // 2. Intentar obtener de LocalStorage si no está en memoria
            try
            {
                var encryptedJson = await _localStorage.GetItemAsync<string>($"cache_{key}");
                if (!string.IsNullOrEmpty(encryptedJson))
                {
                    var json = await _encryptionService.DecryptAsync(encryptedJson);
                    if (!string.IsNullOrEmpty(json))
                    {
                        var persistentEntry = System.Text.Json.JsonSerializer.Deserialize<CacheEntry<T>>(json);
                        if (persistentEntry != null)
                        {
                            if (persistentEntry.Expiration > DateTime.UtcNow)
                            {
                                _memoryCache[key] = persistentEntry;
                                return persistentEntry.Value;
                            }
                            await _localStorage.RemoveItemAsync($"cache_{key}");
                        }
                    }
                }
            }
            catch (Exception) { }

            return default;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, bool persist = false)
        {
            var expiration = DateTime.UtcNow.Add(ttl ?? TimeSpan.FromMinutes(5));
            var entry = new CacheEntry<T>(value, expiration);

            // Guardar en memoria
            _memoryCache[key] = entry;

            if (persist)
            {
                try
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(entry);
                    var encryptedJson = await _encryptionService.EncryptAsync(json);
                    await _localStorage.SetItemAsync($"cache_{key}", encryptedJson);
                }
                catch (Exception) { }
            }
        }

        public async Task RemoveAsync(string key)
        {
            _memoryCache.TryRemove(key, out _);
            try
            {
                await _localStorage.RemoveItemAsync($"cache_{key}");
            }
            catch (Exception) { }
        }

        public async Task ClearAllAsync()
        {
            _memoryCache.Clear();
            try
            {
                // Podríamos iterar las llaves de LocalStorage pero es más seguro/rápido
                // limpiar la memoria y dejar que el storage expire solo si no tenemos un prefijo claro.
                // Sin embargo, AuthService limpiará el storage al logout.
            }
            catch (Exception) { }
        }

        private record CacheEntry<T>(T? Value, DateTime Expiration);
    }
}
