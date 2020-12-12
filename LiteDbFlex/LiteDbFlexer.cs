using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LiteDbFlex {

    public interface ILiteDbFlexer : IDisposable {

        void DropCollection();
    }

    /// <summary>
    ///     litedb flexer (implement chain method and helper class)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LiteDbFlexer<T> : ILiteDbFlexer
        where T : class {
        public ILiteDatabase LiteDatabase { get; }
        public ILiteCollection<T> LiteCollection { get; }
        public bool IsBeginTrans { get; private set; }
        public bool IsCommitted { get; private set; }
        public object Result { get; private set; }
        public string TableName { get; }
        public string DbFileName { get; }
        public string FullDbFileName { get; }

        private static HashSet<LiteDbFlexer<T>> _liteDbFlexers = new HashSet<LiteDbFlexer<T>>();

        public LiteDbFlexer(string additionalDbFileName = "") {
            FullDbFileName = DbFileName =
                typeof(T).GetAttributeValue((LiteDbTableAttribute tableAttribute) => tableAttribute.FileName);

            TableName = typeof(T).GetAttributeValue((LiteDbTableAttribute tableAttribute) => tableAttribute.TableName);
            if (!string.IsNullOrEmpty(additionalDbFileName))
                FullDbFileName = $"{additionalDbFileName}_{DbFileName}";

            var exists = _liteDbFlexers.Where(m => m.FullDbFileName == FullDbFileName).FirstOrDefault();
            if(exists == null) {
                LiteDatabase = LiteDbResolver.Resolve<T>(additionalDbFileName);
                LiteCollection = LiteDatabase.GetCollection<T>(TableName);

                var indexes = typeof(T).GetAttributeValue((LiteDbTableAttribute indexAttribute) => indexAttribute.Indexes);
                foreach (var index in indexes) {
                    LiteCollection.EnsureIndex(index.Key, index.Value);
                }

                _liteDbFlexers.Add(this);
            }
            else {
                LiteDatabase = exists.LiteDatabase;
                LiteCollection = exists.LiteCollection;
            }
        }

        public void DropCollection() {
            LiteDatabase.DropCollection(TableName);
        }

        public void Dispose()
        {
            LiteDatabase?.Dispose();
        }

        public LiteDbFlexer<T> BeginTrans() {
            IsBeginTrans = LiteDatabase.BeginTrans();
            return this;
        }

        public LiteDbFlexer<T> Commit() {
            IsCommitted = LiteDatabase.Commit();
            return this;
        }

        public LiteDbFlexer<T> Rollback() {
            if (IsBeginTrans) LiteDatabase.Rollback();
            return this;
        }

        public LiteDbFlexer<T> EnsureIndex<TK>(Expression<Func<T, TK>> keySelector, bool unique = false) {
            LiteCollection.EnsureIndex(keySelector, unique);
            return this;
        }

        public LiteDbFlexer<T> DropIndex(string name) {
            LiteCollection.DropIndex(name);
            return this;
        }

        public LiteDbFlexer<T> Gets() {
            Result = LiteCollection.FindAll().ToList();
            return this;
        }

        public LiteDbFlexer<T> Gets(Expression<Func<T, bool>> predicate, int skip = 0, int limit = int.MaxValue) {
            if (predicate != null)
                Result = LiteCollection.Find(predicate, skip, limit).ToList();
            else
                Result = LiteCollection.FindAll().Skip(skip).Take(limit).ToList();
            return this;
        }

        public IEnumerable<T> GetEnumerable(Expression<Func<T, bool>> predicate) {
            return LiteCollection.Find(predicate);
        }

        public LiteDbFlexer<T> Get(int id) {
            Result = LiteCollection.FindById(id);
            return this;
        }

        public LiteDbFlexer<T> Get(Expression<Func<T, bool>> predicate) {
            Result = LiteCollection.FindOne(predicate);
            return this;
        }

        public LiteDbFlexer<T> Insert(T entity) {
            Result = LiteCollection.Insert(entity);
            Commited();
            return this;
        }

        public LiteDbFlexer<T> Insert(IEnumerable<T> entities, int batchSize = 5000) {
            Result = LiteCollection.InsertBulk(entities, batchSize);
            Commited();
            return this;
        }

        public LiteDbFlexer<T> Update(T entity) {
            Result = LiteCollection.Update(entity);
            Commited();
            return this;
        }

        public LiteDbFlexer<T> Update(Expression<Func<T, T>> expression, Expression<Func<T, bool>> predicate) {
            Result = LiteCollection.UpdateMany(expression, predicate);
            Commited();
            return this;
        }

        public LiteDbFlexer<T> Delete(int id) {
            Result = LiteCollection.Delete(id);
            Commited();
            return this;
        }

        public LiteDbFlexer<T> Delete(Expression<Func<T, bool>> predicate) {
            Result = LiteCollection.DeleteMany(predicate);
            Commited();
            return this;
        }

        public LiteDbFlexer<T> Delete() {
            Result = LiteCollection.DeleteAll();
            Commited();
            return this;
        }

        public TResult GetResult<TResult>() {
            return (TResult)Result;
        }

        private void Commited() {
            if (IsBeginTrans && !IsCommitted) LiteDatabase.Commit();
        }
    }
}