using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Impostor.Metrics.Gc.Collector.Duration
{
    public sealed class ConcurrentCache<TKey, TValue> : IDisposable
    {
        private readonly ConcurrentDictionary<TKey, CacheValue<TValue>> _cache;
        private readonly CancellationTokenSource _cancellationSource;
        private readonly Task _cleanupTask;
        private readonly TimeSpan _expireItemsAfter;

        internal ConcurrentCache(TimeSpan expireItemsAfter, int initialCapacity = 32)
        {
            _expireItemsAfter = expireItemsAfter;
            if (expireItemsAfter == TimeSpan.Zero) throw new ArgumentNullException(nameof(expireItemsAfter));

            _cache = new ConcurrentDictionary<TKey, CacheValue<TValue>>(Environment.ProcessorCount, initialCapacity);
            _cancellationSource = new CancellationTokenSource();

            _cleanupTask = Task.Run(async () =>
            {
                while (!_cancellationSource.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(expireItemsAfter, _cancellationSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        // exiting.
                        return;
                    }

                    CleanupExpiredValues();
                }
            });
        }

        public void Dispose()
        {
            _cancellationSource.Cancel();
            _cleanupTask.Wait();
        }

        internal void Set(TKey key, TValue value, DateTime? timeStamp = null)
        {
            var cacheValue = new CacheValue<TValue>(value, timeStamp);

            _cache.AddOrUpdate(key, cacheValue, (_, __) => cacheValue);
        }

        internal bool TryGetValue(TKey key, out TValue value, out DateTime timeStamp)
        {
            if (_cache.TryGetValue(key, out var cacheValue))
            {
                value = cacheValue.Value;
                timeStamp = cacheValue.TimeStamp;
                return true;
            }

            value = default;
            timeStamp = default;
            return false;
        }

        internal bool TryRemove(TKey key, out TValue value, out DateTime timeStamp)
        {
            if (_cache.TryRemove(key, out var cacheValue))
            {
                value = cacheValue.Value;
                timeStamp = cacheValue.TimeStamp;
                return true;
            }

            value = default;
            timeStamp = default;
            return false;
        }

        private void CleanupExpiredValues()
        {
            var earliestAddedTime = DateTime.UtcNow.Subtract(_expireItemsAfter);

            foreach (var key in _cache.Keys.ToArray())
            {
                if (!_cache.TryGetValue(key, out var value))
                    continue;

                if (value.TimeStamp < earliestAddedTime)
                    _cache.TryRemove(key, out _);
            }
        }

        internal readonly struct CacheValue<T>
        {
            public CacheValue(T value, DateTime? timeStamp)
            {
                Value = value;
                TimeStamp = timeStamp ?? DateTime.UtcNow;
            }

            public DateTime TimeStamp { get; }
            public T Value { get; }
        }
    }
}