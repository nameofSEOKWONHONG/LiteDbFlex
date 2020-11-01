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
    /// litedbsafeflexer instance and result data cache
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    public sealed class LiteDbCacheFlexer<TEntity> : IDisposable
        where TEntity : class {
        #region property
        /// <summary>
        /// lazy instance of LiteDbCacheSafeFlexer
        /// </summary>
        public static Lazy<LiteDbCacheFlexer<TEntity>> Instance = new Lazy<LiteDbCacheFlexer<TEntity>>(() => {
            return new LiteDbCacheFlexer<TEntity>();
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
        /// async base lock
        /// </summary>
        AsyncLock _mutex = new AsyncLock();
        #endregion

        #region methods
        public LiteDbCacheFlexer<TEntity> SetAdditionalDbFileName(string additionalDbFileName = "") {
            this._additionalDbFileName = additionalDbFileName;
            return this;
        }

        public void SetCache(CacheInfo<TEntity> cacheInfo) {
            var cache = LiteDbFlexerManager.Instance.Value
                .Create<CacheInfo<TEntity>>(this._additionalDbFileName)
                .Get(x => x.CacheName == cacheInfo.CacheName)
                .GetResult<CacheInfo<TEntity>>();

            if(cache != null) {
                if ((DateTime.Now - cache.SetTime).TotalSeconds > cache.Interval) {
                    LiteDbFlexerManager.Instance.Value
                        .Create<CacheInfo<TEntity>>(this._additionalDbFileName)
                        .Delete(cache.Id);
                }
            }

            LiteDbFlexerManager.Instance.Value
                .Create<CacheInfo<TEntity>>(this._additionalDbFileName)
                .Insert(cacheInfo);
        }

        public CacheInfo<TEntity> GetCache(string cacheName, CacheInfo<TEntity> cacheInfo = null) {
            var cache = LiteDbFlexerManager.Instance.Value.Create<CacheInfo<TEntity>>(this._additionalDbFileName).Get(x => x.CacheName == cacheName).GetResult<CacheInfo<TEntity>>();
            if (cache != null) {
                if ((DateTime.Now - cache.SetTime).TotalSeconds > cache.Interval) {
                    LiteDbFlexerManager.Instance.Value
                        .Create<CacheInfo<TEntity>>(this._additionalDbFileName)
                        .Delete(cache.Id);
                } else {
                    return cache;
                }
            }

            return null;
        }

        private bool IsDiff(object diff1, object diff2) {
            var diff1HashCode = diff1.jToHashCode();
            var diff2HashCode = diff2.jToHashCode();

            if (diff1HashCode != diff2HashCode) return true;
            return false;
        }

        public void DropCollection() {
            lock (_lock) {
                LiteDbFlexerManager.Instance.Value.Create<TEntity>(this._additionalDbFileName).DropCollection();
            }
        }

        public async Task DropCollectionAsync() {
            using(await _mutex.LockAsync()) {
                LiteDbFlexerManager.Instance.Value.Create<TEntity>(this._additionalDbFileName).DropCollection();
            }
        }

        public void Dispose() {

        }
        #endregion
    }

    #region cacheinfo class
    [LiteDbTable("cache.db", "caches")]
    [LiteDbIndex(new[] { "CacheName", "HashCode" })]
    public class CacheInfo<TEntity> {
        public int Id { get; set; }
        public string CacheName { get; set; }
        public int HashCode { get; set; }
        public TEntity Data { get; set; }
        public DateTime SetTime { get; set; }
        public int Interval { get; set; }

        public CacheInfo() { }

        public CacheInfo(string cacheName, TEntity data, DateTime setTime, int interval) {
            this.CacheName = cacheName;
            this.Data = data;
            this.SetTime = setTime;
            this.Interval = interval;
        }
    }
    #endregion
}
