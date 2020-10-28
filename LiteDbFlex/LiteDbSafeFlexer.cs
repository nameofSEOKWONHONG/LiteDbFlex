using Nito.AsyncEx;
using System;
using System.Threading.Tasks;

namespace LiteDbFlex {
    public class LiteDbSafeFlexer<T>
        where T : class {
        string _additionalDbFileName = string.Empty;
        object _lock = new object();
        AsyncLock _mutex = new AsyncLock();

        public static Lazy<LiteDbSafeFlexer<T>> Instance = new Lazy<LiteDbSafeFlexer<T>>(() => {
            return new LiteDbSafeFlexer<T>();
        });

        public LiteDbSafeFlexer<T> SetAdditionalDbFileName(string additionalDbFileName = "") {
            this._additionalDbFileName = additionalDbFileName;
            return this;
        }

        public TResult Execute<TResult>(Func<LiteDbFlexer<T>, TResult> func) {
            TResult result = default(TResult);
            lock (_lock) {
                var liteDbFlexer = new LiteDbFlexer<T>(_additionalDbFileName);
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

        public async Task<TResult> ExecuteAsync<TResult>(Func<LiteDbFlexer<T>, TResult> func) {
            TResult result = default(TResult);

            using (await _mutex.LockAsync()) {
                var liteDbFlexer = new LiteDbFlexer<T>(_additionalDbFileName);
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
