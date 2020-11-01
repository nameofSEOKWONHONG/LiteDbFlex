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
    public class LiteDbFlexerManager {

        public static Lazy<LiteDbFlexerManager> Instance = new Lazy<LiteDbFlexerManager>(() => {
            return new LiteDbFlexerManager();
        });

        List<LiteDbFlexManageInfo> _liteDbFlexManageInfos = new List<LiteDbFlexManageInfo>();

        public LiteDbFlexer<TEntity> Create<TEntity>(string additionalDbFileName = "")
            where TEntity : class {
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
            return (LiteDbFlexer<TEntity>)exists.LiteDbFlexer;
        }

        public void DropCollection() {
            _liteDbFlexManageInfos.ForEach(item => {
                item.LiteDbFlexer.DropCollection();
            });
        }

        public void Dispose() {
            _liteDbFlexManageInfos.ForEach(item => {
                item.LiteDbFlexer.Dispose();
            });
            _liteDbFlexManageInfos.Clear();
        }

        public class LiteDbFlexManageInfo {
            public string LiteDbName { get; set; }
            public ILiteDbFlexer LiteDbFlexer { get; set; }
        }
    }
}
