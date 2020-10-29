using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiteDbFlex {
    /// <summary>
    /// litedb v5 support multi access(crud). (Only simultaneous creation is not supported.)
    /// therefore, It is maintained for an instance once created.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class LiteDbFlexerManager<TEntity>
        where TEntity : class {

        public static Lazy<LiteDbFlexerManager<TEntity>> Instance = new Lazy<LiteDbFlexerManager<TEntity>>(() => {
            return new LiteDbFlexerManager<TEntity>();
        });

        List<LiteDbFlexManageInfo> _liteDbFlexManageInfos = new List<LiteDbFlexManageInfo>();

        public LiteDbFlexer<TEntity> Create(string additionalDbFileName = "") {
            var dbFileName = typeof(TEntity).GetAttributeValue((LiteDbTableAttribute tableAttribute) => tableAttribute.FileName);
            if (!string.IsNullOrEmpty(additionalDbFileName)) {
                dbFileName = $"{additionalDbFileName}_{dbFileName}";
            }
            var exists = _liteDbFlexManageInfos.Where(m => m.LiteDbName == dbFileName).FirstOrDefault();
            if(exists == null) {
                exists = new LiteDbFlexManageInfo() {
                    LiteDbName = dbFileName,
                    LiteDbFlexer = new LiteDbFlexer<TEntity>(additionalDbFileName)
                };
                _liteDbFlexManageInfos.Add(exists);
            }
            return exists.LiteDbFlexer;
        }

        public void Dispose() {
            _liteDbFlexManageInfos.ForEach(item => {
                item.LiteDbFlexer.Dispose();
            });
            _liteDbFlexManageInfos.Clear();
        }

        public class LiteDbFlexManageInfo {
            public string LiteDbName { get; set; }
            public LiteDbFlexer<TEntity> LiteDbFlexer { get; set; }
        }
    }
}
