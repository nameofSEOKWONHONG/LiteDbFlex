using Nito.AsyncEx;
using System;
using System.Threading.Tasks;

namespace LiteDbFlex {
    public class LiteDbSafeFlexer<T>
        where T : class {
        string _additionalName = string.Empty;
        object _lock = new object();
        AsyncLock _mutex = new AsyncLock();

        public static Lazy<LiteDbSafeFlexer<T>> Instance = new Lazy<LiteDbSafeFlexer<T>>(() => {
            return new LiteDbSafeFlexer<T>();
        });

        public LiteDbSafeFlexer<T> SetAdditionalName(string additionalName = "") {
            this._additionalName = additionalName;
            return this;
        }

        public TResult Execute<TResult>(Func<LiteDbFlexer<T>, TResult> func) {
            TResult result = default(TResult);
            lock (_lock) {
                var litedbInstnace = new LiteDbFlexer<T>(_additionalName);
                try {
                    result = func(litedbInstnace);
                } catch {
                    if (litedbInstnace.IsTran) {
                        litedbInstnace.Rollback();
                    }
                }
                litedbInstnace.Dispose();
            }

            return result;
        }

        public async Task<TResult> ExecuteAsync<TResult>(Func<LiteDbFlexer<T>, TResult> func) {
            TResult result = default(TResult);

            using (await _mutex.LockAsync()) {
                var litedbInstnace = new LiteDbFlexer<T>(_additionalName);
                try {
                    result = func(litedbInstnace);
                }
                catch {
                    if(litedbInstnace.IsTran) {
                        litedbInstnace.Rollback();
                    }
                }
                litedbInstnace.Dispose();
            }

            return result;
        }
    }
}
