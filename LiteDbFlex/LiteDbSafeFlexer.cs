using Newtonsoft.Json;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiteDbFlex {

    /// <summary>
    /// litedb instance safe class
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public sealed class LiteDbSafeFlexer<TEntity>
        where TEntity : class {
        string _additionalDbFileName = string.Empty;
        object _lock = new object();
        AsyncLock _mutex = new AsyncLock();

        public static Lazy<LiteDbSafeFlexer<TEntity>> Instance = new Lazy<LiteDbSafeFlexer<TEntity>>(() => {
            return new LiteDbSafeFlexer<TEntity>();
        });

        public LiteDbSafeFlexer<TEntity> SetAdditionalDbFileName(string additionalDbFileName = "") {
            this._additionalDbFileName = additionalDbFileName;
            return this;
        }

        public TResult Execute<TResult>(Func<LiteDbFlexer<TEntity>, TResult> func) {
            TResult result = default(TResult);
            lock (_lock) {
                var liteDbFlexer = new LiteDbFlexer<TEntity>(_additionalDbFileName);
                try {
                    result = func(liteDbFlexer);
                } catch {
                    if (liteDbFlexer.IsTran) {
                        liteDbFlexer.Rollback();
                    }
                }
                liteDbFlexer.Dispose();
            }

            return result;
        }

        public async Task<TResult> ExecuteAsync<TResult>(Func<LiteDbFlexer<TEntity>, TResult> func) {
            TResult result = default(TResult);

            using (await _mutex.LockAsync()) {
                var liteDbFlexer = new LiteDbFlexer<TEntity>(_additionalDbFileName);
                try {
                    result = func(liteDbFlexer);
                }
                catch {
                    if(liteDbFlexer.IsTran) {
                        liteDbFlexer.Rollback();
                    }
                }
                liteDbFlexer.Dispose();
            }

            return result;
        }
    }
}
