using System;
using System.Collections.Generic;
using System.Linq;

namespace Ya.D.Services.HTTP
{
    public class CachingService
    {
        private readonly TimeSpan _expiration = TimeSpan.FromHours(1);
        private Dictionary<string, CacheItem> _cache = new Dictionary<string, CacheItem>();
        private static readonly Lazy<CachingService> _instance = new Lazy<CachingService>(() => new CachingService());

        public static CachingService Cache { get => _instance.Value; }

        private CachingService() { }

        public void AddItem(string key, object data)
        {
            CheckExpired();
            if (_cache.ContainsKey(key))
                _cache[key] = new CacheItem { Data = data };
            else
                _cache.Add(key, new CacheItem { Data = data });
        }

        public object GetItem(string key)
        {
            CheckExpired();
            if (!_cache.ContainsKey(key))
                return null;
            return _cache[key].Data;
        }

        public T GetItem<T>(string key) where T : class
        {
            CheckExpired();
            if (!_cache.ContainsKey(key))
                return null;
            return _cache[key].Data as T;
        }

        private void CheckExpired()
        {
            foreach (var item in _cache.Where(i => i.Value.ModifyDate - DateTime.Now > _expiration))
                _cache.Remove(item.Key);
        }

        private class CacheItem
        {
            public object Data { get; set; }
            public DateTime ModifyDate { get; set; } = DateTime.Now;
        }
    }
}