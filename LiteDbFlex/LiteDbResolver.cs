using LiteDB;

namespace LiteDbFlex {
    public class LiteDbResolver {
        private LiteDbResolver() {

        }

        public static ILiteDatabase Resolve<TEntity>()
            where TEntity : class {
            var fileConnection = typeof(TEntity).GetAttributeValue((LiteDbTableAttribute tableAttribute) => tableAttribute.FileName);
            if (!string.IsNullOrEmpty(fileConnection)) {
                return new LiteDatabase(fileConnection);
            }

            return null;
        }
    }
}
