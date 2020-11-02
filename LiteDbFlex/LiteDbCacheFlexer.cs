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
    /// <typeparam name="TCache"></typeparam>
    /// <typeparam name="TRequest"></typeparam>
    public sealed class LiteDbCacheFlexer : IDisposable {
        #region property
        /// <summary>
        /// lazy instance of LiteDbCacheSafeFlexer
        /// </summary>
        public static Lazy<LiteDbCacheFlexer> Instance = new Lazy<LiteDbCacheFlexer>(() => {
            return new LiteDbCacheFlexer();
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
        public LiteDbCacheFlexer SetAdditionalDbFileName(string additionalDbFileName = "") {
            this._additionalDbFileName = additionalDbFileName;
            return this;
        }

        /// <summary>
        /// set cache (if exists same cache name, after delete exists cache, insert new cahe.)
        /// </summary>
        /// <param name="func"></param>
        public void SetCache<TEntity>(Func<CacheInfo<TEntity>> func)
            where TEntity : class{
            var cacheInfo = func();
            var cache = LiteDbFlexerManager.Instance.Value
                .Create<CacheInfo<TEntity>>(this._additionalDbFileName)
                .Get(x => x.CacheName == cacheInfo.CacheName)
                .GetResult<CacheInfo<TEntity>>();

            if(cache != null) {
                if(cache.SetTime.HasValue) {
                    if ((DateTime.Now - cache.SetTime.Value).TotalSeconds > cache.Interval) {
                        LiteDbFlexerManager.Instance.Value
                            .Create<CacheInfo<TEntity>>(this._additionalDbFileName)
                            .Delete(cache.Id);
                    }
                }
                else {
                    LiteDbFlexerManager.Instance.Value
                        .Create<CacheInfo<TEntity>>(this._additionalDbFileName)
                        .Delete(cache.Id);
                }
            }

            LiteDbFlexerManager.Instance.Value
                .Create<CacheInfo<TEntity>>(this._additionalDbFileName)
                .Insert(cacheInfo);
        }

        public CacheInfo<TEntity> GetCache<TEntity>(string cacheName)
            where TEntity : class {
            var cache = LiteDbFlexerManager.Instance.Value.Create<CacheInfo<TEntity>>(this._additionalDbFileName).Get(x => x.CacheName == cacheName).GetResult<CacheInfo<TEntity>>();
            if (cache != null) {
                if(cache.SetTime.HasValue) {
                    if ((DateTime.Now - cache.SetTime.Value).TotalSeconds > cache.Interval) {
                        LiteDbFlexerManager.Instance.Value
                            .Create<CacheInfo<TEntity>>(this._additionalDbFileName)
                            .Delete(cache.Id);
                        cache.EnumCacheState = ENUM_CACHE_STATE.DELETED;
                    }
                }
            }

            return cache;
        }

        private bool IsDiff(object diff1, object diff2) {
            var diff1HashCode = diff1.jToHashCode();
            var diff2HashCode = diff2.jToHashCode();

            if (diff1HashCode != diff2HashCode) return true;
            return false;
        }

        public void DropCollection() {
            lock (_lock) {
                LiteDbFlexerManager.Instance.Value.DropCollection();
            }
        }

        public void DropCollection<TEntity>() {
            lock(_lock) {
                LiteDbFlexerManager.Instance.Value.Create<CacheInfo<TEntity>>(this._additionalDbFileName).DropCollection();
            }
        }

        public async Task DropCollectionAsync() {
            using(await _mutex.LockAsync()) {
                LiteDbFlexerManager.Instance.Value.DropCollection();
            }
        }

        public void Dispose() {
            lock(_lock) {
                LiteDbFlexerManager.Instance.Value.Dispose();
            }
        }
        #endregion
    }

    #region cacheinfo class
    [LiteDbTable("cache.db", "caches")]
    [LiteDbIndex(new[] { "CacheName" })]
    public class CacheInfo<TEntity> {
        /// <summary>
        /// Id (litedb id)
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// CacheName (Key)
        /// </summary>
        public string CacheName { get; set; }
        /// <summary>
        /// Data (Value)
        /// </summary>
        public TEntity Data { get; set; }
        /// <summary>
        /// SetTime (default DateTime.Now)
        /// if this property is null, never delete.
        /// </summary>
        public DateTime? SetTime { get; set; }
        /// <summary>
        /// Interval (delete interval)
        /// </summary>
        public int Interval { get; set; }
        /// <summary>
        /// Cache State (normal, deleted)
        /// </summary>
        public ENUM_CACHE_STATE EnumCacheState { get; set; } = ENUM_CACHE_STATE.NORMAL;

        public CacheInfo() { }

        public CacheInfo(string cacheName, TEntity data, DateTime setTime, int interval) {
            this.CacheName = cacheName;
            this.Data = data;
            this.SetTime = setTime;
            this.Interval = interval;
            this.EnumCacheState = ENUM_CACHE_STATE.NORMAL;
        }
    }

    public enum ENUM_CACHE_STATE {
        NORMAL,
        DELETED
    }
    #endregion
}
