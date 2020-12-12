using LiteDB;
using System.Collections.Generic;
using System.Linq;

namespace LiteDbFlex {

    /// <summary>
    ///     create litedb instance
    /// </summary>
    internal class LiteDbResolver {

        private LiteDbResolver() {
        }

        public static ILiteDatabase Resolve<TEntity>(string additionalDbFileName = "")
            where TEntity : class {
            var fileConnection =
                typeof(TEntity).GetAttributeValue((LiteDbTableAttribute tableAttribute) => tableAttribute.FileName);

            if (!string.IsNullOrEmpty(fileConnection)) {
                if (!string.IsNullOrEmpty(additionalDbFileName))
                    return new LiteDatabase($"{additionalDbFileName}_{fileConnection}");
                return new LiteDatabase(fileConnection);
            }

            return null;
        }
    }
}