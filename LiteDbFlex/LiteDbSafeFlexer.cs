using Nito.AsyncEx;
using System;
using System.Threading.Tasks;

namespace LiteDbFlex {

    /// <summary>
    /// litedb instance safe class
    /// use locking for single instance
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public sealed class LiteDbSafeFlexer<TEntity>
        where TEntity : class
    {

        public static readonly Lazy<LiteDbSafeFlexer<TEntity>> Instance =
            new Lazy<LiteDbSafeFlexer<TEntity>>(() => new LiteDbSafeFlexer<TEntity>());

        private string _additionalDbFileName = string.Empty;
        private readonly object _lock = new object();
        private readonly AsyncLock _mutex = new AsyncLock();

        private LiteDbSafeFlexer()
        {
        }

        public LiteDbSafeFlexer<TEntity> SetAdditionalDbFileName(string additionalDbFileName = "") {
            _additionalDbFileName = additionalDbFileName;
            return this;
        }

        public TResult Execute<TResult>(Func<LiteDbFlexer<TEntity>, TResult> func) {
            TResult result = default;
            lock (_lock) {
                var liteDbFlexer = new LiteDbFlexer<TEntity>(_additionalDbFileName);
                try {
                    result = func(liteDbFlexer);
                } catch {
                    if (liteDbFlexer.IsBeginTrans) liteDbFlexer.Rollback();
                }

                liteDbFlexer.Dispose();
            }

            return result;
        }

        public async Task<TResult> ExecuteAsync<TResult>(Func<LiteDbFlexer<TEntity>, TResult> func) {
            TResult result = default;

            using (await _mutex.LockAsync()) {
                var liteDbFlexer = new LiteDbFlexer<TEntity>(_additionalDbFileName);
                try {
                    result = func(liteDbFlexer);
                } catch {
                    if (liteDbFlexer.IsBeginTrans) liteDbFlexer.Rollback();
                }

                liteDbFlexer.Dispose();
            }

            return result;
        }
    }
}