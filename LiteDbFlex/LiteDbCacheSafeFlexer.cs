using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LiteDbFlex {
    /// <summary>
    /// litedb instance safe and result data cache
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    public sealed class LiteDbCacheSafeFlexer<TEntity, TRequest>
        where TEntity : class
        where TRequest : class {
        #region property
        /// <summary>
        /// cache list
        /// </summary>
        public List<CacheInfo> Caches { get; private set; } = new List<CacheInfo>();
        /// <summary>
        /// lazy instance of LiteDbCacheSafeFlexer
        /// </summary>
        public static Lazy<LiteDbCacheSafeFlexer<TEntity, TRequest>> Instance = new Lazy<LiteDbCacheSafeFlexer<TEntity, TRequest>>(() => {
            return new LiteDbCacheSafeFlexer<TEntity, TRequest>();
        });
        #endregion

        #region private
        /// <summary>
        /// additional db file name
        /// </summary>
        string _additionalDbFileName = string.Empty;

        /// <summary>
        /// base lock
        /// </summary>
        object _lock = new object();

        /// <summary>
        /// cache lock
        /// </summary>
        object _cacheClearLock = new object();

        /// <summary>
        /// async base lock
        /// </summary>
        AsyncLock _mutex = new AsyncLock();

        /// <summary>
        /// async cache lock
        /// </summary>
        AsyncLock _cacheClearMutex = new AsyncLock();

        /// <summary>
        /// request
        /// </summary>
        TRequest _request = null;

        /// <summary>
        /// cache clear counter
        /// </summary>
        int _cacheClearCounter = 0;
        #endregion

        #region private const
        /// <summary>
        /// cache clear time interval
        /// </summary>
        const int INTERVAL = 5; //sec

        /// <summary>
        /// cache clear counter limit (per 5000)
        /// </summary>
        const int COUNTER_LIMIT = 5000;
        #endregion

        #region methods
        public LiteDbCacheSafeFlexer<TEntity, TRequest> SetAdditionalDbFileName(string additionalDbFileName = "") {
            this._additionalDbFileName = additionalDbFileName;
            return this;
        }

        public LiteDbCacheSafeFlexer<TEntity, TRequest> SetRequest(TRequest request) {
            this._request = request;
            var requestHash = this._request.jToHashCode();
            var selected = Caches.Where(m => m.HashCode == requestHash).FirstOrDefault();
            if (selected == null) {
                Caches.Add(new CacheInfo(requestHash, null, DateTime.Now));
            }
            return this;
        }

        public TResult Execute<TResult>(Func<LiteDbFlexer<TEntity>, TRequest, TResult> func) {
            CheckCacheClear();
            this._cacheClearCounter += 1;
            var requestHash = this._request.jToHashCode();
            var cache = Caches.Where(m => m.HashCode == requestHash).FirstOrDefault();
            if (cache != null) {
                var interval = (DateTime.Now - cache.SetTime.Value).TotalSeconds;
                if (interval <= INTERVAL) {
                    if (cache.HashCode != 0 && cache.Data != null) {
                        return (TResult)cache.Data;
                    }
                } else {
                    cache.Data = null;
                }
            }

            TResult result = default(TResult);
            lock (_lock) {
                var liteDbFlexer = new LiteDbFlexer<TEntity>(_additionalDbFileName);
                try {
                    result = func(liteDbFlexer, _request);
                    if(IsDiff(result, cache.Data)) {
                        cache.Data = null;
                        cache.Data = result;
                        cache.SetTime = DateTime.Now;
                    }
                } catch {
                    if (liteDbFlexer.IsTran) {
                        liteDbFlexer.Rollback();
                    }
                }
                liteDbFlexer.Dispose();
            }

            return result;
        }

        public async Task<TResult> ExecuteAsync<TResult>(Func<LiteDbFlexer<TEntity>, TRequest, TResult> func) {
            await CheckCacheClearAsync();
            this._cacheClearCounter += 1;
            var requestHash = this._request.jToHashCode();
            var cache = Caches.Where(m => m.HashCode == requestHash).FirstOrDefault();
            if (cache != null) {
                var interval = (DateTime.Now - cache.SetTime.Value).TotalSeconds;
                if (interval <= INTERVAL) {
                    if (cache.HashCode != 0 && cache.Data != null) {
                        return (TResult)cache.Data;
                    }
                }
                else {
                    cache.Data = null;
                }
            }

            TResult result = default(TResult);
            using (await _mutex.LockAsync()) {
                var liteDbFlexer = new LiteDbFlexer<TEntity>(_additionalDbFileName);
                try {
                    result = func(liteDbFlexer, _request);
                    if (IsDiff(result, cache.Data)) {
                        cache.Data = null;
                        cache.Data = result;
                        cache.SetTime = DateTime.Now;
                    }
                } catch {
                    if (liteDbFlexer.IsTran) {
                        liteDbFlexer.Rollback();
                    }
                }
                liteDbFlexer.Dispose();
            }

            return result;
        }

        private bool IsDiff(object diff1, object diff2) {
            var diff1HashCode = diff1.jToHashCode();
            var diff2HashCode = diff2.jToHashCode();

            if (diff1HashCode != diff2HashCode) return true;
            return false;
        }

        private void CheckCacheClear() {
            lock(_cacheClearLock) {
                if (_cacheClearCounter >= COUNTER_LIMIT) {
                    ClearCache();
                    _cacheClearCounter = 0;
                }
            }
        }

        public void ClearCache() {
            lock (_lock) {
                var removes = new List<CacheInfo>();
                foreach (var item in Caches) {
                    if (item.SetTime == null) continue;
                    if ((DateTime.Now - item.SetTime.Value).TotalSeconds > INTERVAL) {
                        removes.Add(item);
                    }
                }

                for (var i = 0; i < removes.Count; i++) {
                    Caches.Remove(removes[i]);
                }
            }
        }

        private async Task CheckCacheClearAsync() {
            using(await _cacheClearMutex.LockAsync()) {
                if (_cacheClearCounter >= COUNTER_LIMIT) {
                    await ClearCacheAsync();
                    _cacheClearCounter = 0;
                }
            }
        }

        public async Task ClearCacheAsync() {
            using(await _mutex.LockAsync()) {
                var removes = new List<CacheInfo>();
                foreach (var item in Caches) {
                    if (item.SetTime == null) continue;
                    if ((DateTime.Now - item.SetTime.Value).TotalSeconds > INTERVAL) {
                        removes.Add(item);
                    }
                }

                for (var i = 0; i < removes.Count; i++) {
                    Caches.Remove(removes[i]);
                }
            }
        }
        #endregion

        #region cacheinfo class
        public class CacheInfo {
            public int HashCode { get; set; } = 0;
            public object Data { get; set; } = null;
            public DateTime? SetTime { get; set; } = null;

            public CacheInfo(int hashCode, object data, DateTime? setTime) {
                this.HashCode = hashCode;
                this.Data = data;
                this.SetTime = setTime;
            }
        }
        #endregion

    }
}
