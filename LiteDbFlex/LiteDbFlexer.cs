using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LiteDbFlex {
    public static class LiteDbFlexer {
        #region [litedb - chaining methods]
        public static ILiteCollection<T> jGetCollection<T>(this ILiteDatabase liteDatabase, string tableName)
            where T : class {
            return liteDatabase.GetCollection<T>(tableName);
        }
        public static ILiteCollection<T> jGetCollection<T>(this ILiteDatabase liteDatabase)
            where T : class {
            return liteDatabase.GetCollection<T>(typeof(T).GetAttributeValue((LiteDbTableAttribute tableAttribute) => tableAttribute.TableName));
        }
        public static ILiteDatabase jBeginTrans(this ILiteDatabase liteDatabase) {
            if (liteDatabase.BeginTrans() == false) {
                throw new Exception("litedb transaction failed on begintrans");
            }
            return liteDatabase;
        }

        public static T jGet<T>(this ILiteCollection<T> liteCollection, int id) {
            return liteCollection.FindById(id);
        }

        public static T jGet<T>(this ILiteCollection<T> liteCollection, Expression<Func<T, bool>> expression) {
            return liteCollection.FindOne(expression);
        }

        public static IEnumerable<T> jGetAll<T>(this ILiteCollection<T> liteCollection) {
            return liteCollection.FindAll();
        }

        public static bool jUpdate<T>(this ILiteCollection<T> liteCollection, T entity) {
            return liteCollection.Update(entity);
        }

        public static BsonValue jInsert<T>(this ILiteCollection<T> liteCollection, T entity) {
            return liteCollection.Insert(entity);
        }

        public static int jInsertBulk<T>(this ILiteCollection<T> liteCollection, IEnumerable<T> entities, int bulksize = 5000) {
            return liteCollection.InsertBulk(entities, bulksize);
        }

        public static bool jDelete<T>(this ILiteCollection<T> liteCollection, BsonValue id) {
            return liteCollection.Delete(id);
        }

        public static bool jDeleteAll<T>(this ILiteCollection<T> liteCollection) {
            return liteCollection.DeleteAll() > 0;
        }

        public static bool jDeleteMany<T>(this ILiteCollection<T> liteCollection, Expression<Func<T, bool>> expression) {
            return liteCollection.DeleteMany(expression) > 0;
        }

        public static bool jEnsureIndex<T>(this ILiteCollection<T> liteCollection, Expression<Func<T, T>> expression) {
            return liteCollection.EnsureIndex<T>(expression);
        }

        public static bool jUpsert<T>(this ILiteCollection<T> liteCollection, T entity) {
            return liteCollection.Upsert(entity);
        }

        public static bool jCommit(this ILiteDatabase liteDatabase) {
            var result = liteDatabase.Commit();
            if (result == false) {
                throw new Exception("litedb transaction failed on commit");
            }
            return result;
        }

        public static bool jRollback(this ILiteDatabase liteDatabase) {
            var result = liteDatabase.Rollback();
            if (result == false) {
                throw new Exception("litedb transaction failed on rollback");
            }
            return result;
        }
        #endregion [litedb]
    }
}
