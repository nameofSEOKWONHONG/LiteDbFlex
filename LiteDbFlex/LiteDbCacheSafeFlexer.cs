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
        public List<CacheSafe> Caches { get; private set; } = new List<CacheSafe>();

        string _additionalDbFileName = string.Empty;
        object _lock = new object();
        object _cacheClearLock = new object();

        AsyncLock _mutex = new AsyncLock();
        AsyncLock _cacheClearMutex = new AsyncLock();

        TRequest _request = null;

        const int INTERVAL = 5; //sec
        const int COUNTER_LIMIT = 100000;
        int counter = 0;

        public static Lazy<LiteDbCacheSafeFlexer<TEntity, TRequest>> Instance = new Lazy<LiteDbCacheSafeFlexer<TEntity, TRequest>>(() => {
            return new LiteDbCacheSafeFlexer<TEntity, TRequest>();
        });

        public LiteDbCacheSafeFlexer<TEntity, TRequest> SetAdditionalDbFileName(string additionalDbFileName = "") {
            this._additionalDbFileName = additionalDbFileName;
            return this;
        }

        public LiteDbCacheSafeFlexer<TEntity, TRequest> SetRequest(TRequest request) {
            this._request = request;
            var requestHash = JsonConvert.SerializeObject(request).GetHashCode();
            var selected = Caches.Where(m => m.HashCode == requestHash).FirstOrDefault();
            if (selected == null) {
                Caches.Add(new CacheSafe(requestHash, null, DateTime.Now));
            }
            return this;
        }

        public TResult Execute<TResult>(Func<LiteDbFlexer<TEntity>, TRequest, TResult> func) {
            CheckCacheClear();
            counter += 1;
            var requestHash = JsonConvert.SerializeObject(this._request).GetHashCode();
            var cahce = Caches.Where(m => m.HashCode == requestHash).FirstOrDefault();
            var interval = (DateTime.Now - cahce.SetTime.Value).TotalSeconds;
            if (interval <= INTERVAL) {
                if (cahce.HashCode != 0 && cahce.Data != null) {
                    return (TResult)cahce.Data;
                }
            }
            else {
                cahce.Data = null;
            }

            TResult result = default(TResult);
            lock (_lock) {
                var liteDbFlexer = new LiteDbFlexer<TEntity>(_additionalDbFileName);
                try {
                    result = func(liteDbFlexer, _request);
                    if(IsDiff(result, cahce.Data)) {
                        cahce.Data = null;
                        cahce.Data = result;
                        cahce.SetTime = DateTime.Now;
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
            counter += 1;
            var requestHash = JsonConvert.SerializeObject(this._request).GetHashCode();
            var cahce = Caches.Where(m => m.HashCode == requestHash).FirstOrDefault();
            var interval = (DateTime.Now - cahce.SetTime.Value).TotalSeconds;
            if (interval <= INTERVAL) {
                if (cahce.HashCode != 0 && cahce.Data != null) {
                    return (TResult)cahce.Data;
                }
            }

            TResult result = default(TResult);
            using (await _mutex.LockAsync()) {
                var liteDbFlexer = new LiteDbFlexer<TEntity>(_additionalDbFileName);
                try {
                    result = func(liteDbFlexer, _request);
                    if (IsDiff(result, cahce.Data)) {
                        cahce.Data = null;
                        cahce.Data = result;
                        cahce.SetTime = DateTime.Now;
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
            var diff1String = JsonConvert.SerializeObject(diff1);
            var diff2String = JsonConvert.SerializeObject(diff2);

            if (diff1String.GetHashCode() != diff2String.GetHashCode()) return true;
            return false;
        }

        private void CheckCacheClear() {
            lock(_cacheClearLock) {
                if (counter >= COUNTER_LIMIT) {
                    CacheClear();
                    counter = 0;
                }
            }
        }

        public void CacheClear() {
            lock (_lock) {
                var removes = new List<CacheSafe>();
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
                if (counter >= COUNTER_LIMIT) {
                    await CacheClearAsync();
                    counter = 0;
                }
            }
        }

        public async Task CacheClearAsync() {
            using(await _mutex.LockAsync()) {
                var removes = new List<CacheSafe>();
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

        public class CacheSafe {
            public int HashCode { get; set; } = 0;
            public object Data { get; set; } = null;
            public DateTime? SetTime { get; set; } = null;

            public CacheSafe(int hashCode, object data, DateTime? setTime) {
                this.HashCode = hashCode;
                this.Data = data;
                this.SetTime = setTime;
            }
        }

    }
}
