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
    /// litedb flexer (implement chain method and helper class)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LiteDbFlexer<T> : ILiteDbFlexer
        where T : class {
        public ILiteDatabase LiteDatabase { get; private set; }
        public ILiteCollection<T> LiteCollection { get; private set; }
        public bool IsBeginTrans { get; private set; }
        public bool IsCommitted { get; private set; }
        public object Result { get; private set; }
        public string TableName {get; private set;}
        public string DbFileName { get; private set; }
        public string FullDbFileName { get; private set; }

        public LiteDbFlexer(string additionalDbFileName = "") {
            FullDbFileName = DbFileName = typeof(T).GetAttributeValue((LiteDbTableAttribute tableAttribute) => tableAttribute.FileName);
            TableName = typeof(T).GetAttributeValue((LiteDbTableAttribute tableAttribute) => tableAttribute.TableName);

            if(!string.IsNullOrEmpty(additionalDbFileName)) {
                FullDbFileName = $"{additionalDbFileName}_{DbFileName}";
            }

            this.LiteDatabase = LiteDbResolver.Resolve<T>(additionalDbFileName);
            this.LiteCollection = this.LiteDatabase.GetCollection<T>(TableName);
        }

        public LiteDbFlexer<T> BeginTrans() {
            this.IsBeginTrans = this.LiteDatabase.BeginTrans();
            return this;
        }

        public LiteDbFlexer<T> Commit() {
            this.IsCommitted = this.LiteDatabase.Commit();
            return this;
        }

        public LiteDbFlexer<T> Rollback() {
            if(this.IsBeginTrans) {
                this.LiteDatabase.Rollback();
            }
            return this;
        }

        public LiteDbFlexer<T> EnsureIndex<K>(Expression<Func<T, K>> keySelector, bool unique = false)
        {
            this.LiteCollection.EnsureIndex(keySelector, unique);
            return this;
        }

        public LiteDbFlexer<T> DropIndex(string name) {
            this.LiteCollection.DropIndex(name);
            return this;
        }

        public void DropCollection() {
            this.LiteDatabase.DropCollection(this.TableName);
        }

        public LiteDbFlexer<T> Gets() {
            this.Result = this.LiteCollection.FindAll().ToList<T>();
            return this;
        }

        public LiteDbFlexer<T> Gets(Expression<Func<T, bool>> predicate, int skip = 0, int limit = int.MaxValue) {
            if(predicate != null)
                this.Result = this.LiteCollection.Find(predicate, skip, limit).ToList<T>();
            else
                this.Result = this.LiteCollection.FindAll().Skip(skip).Take(limit).ToList<T>();
            return this;
        }

        public IEnumerable<T> GetEnumerable(Expression<Func<T, bool>> predicate) {
            return this.LiteCollection.Find(predicate);
        }

        public LiteDbFlexer<T> Get(int id) {
            this.Result = this.LiteCollection.FindById(id);
            return this;
        }

        public LiteDbFlexer<T> Get(Expression<Func<T, bool>> predicate) {
            this.Result = this.LiteCollection.FindOne(predicate);
            return this;
        }

        public LiteDbFlexer<T> Insert(T entity) {
            this.Result = this.LiteCollection.Insert(entity);
            Commited();
            return this;
        }

        public LiteDbFlexer<T> Insert(IEnumerable<T> entities, int batchSize = 5000) {
            this.Result = this.LiteCollection.InsertBulk(entities, batchSize);
            Commited();
            return this;
        }

        public LiteDbFlexer<T> Update(T entity) {
            this.Result = this.LiteCollection.Update(entity);
            Commited();
            return this;
        }

        public LiteDbFlexer<T> Update(Expression<Func<T, T>> expression, Expression<Func<T, bool>> predicate) {
            this.Result = this.LiteCollection.UpdateMany(expression, predicate);
            Commited();
            return this;
        }

        public LiteDbFlexer<T> Delete(int id) {
            this.Result = this.LiteCollection.Delete(id);
            Commited();
            return this;
        }

        public LiteDbFlexer<T> Delete(Expression<Func<T, bool>> predicate) {
            this.Result = this.LiteCollection.DeleteMany(predicate);
            Commited();
            return this;
        }

        public LiteDbFlexer<T> Delete() {
            this.Result = this.LiteCollection.DeleteAll();
            Commited();
            return this;
        }

        public TResult GetResult<TResult>() {
            return (TResult)this.Result;
        }

        private void Commited() {
            if (this.IsBeginTrans && !this.IsCommitted) {
                this.LiteDatabase.Commit();
            }
        }

        public void Dispose() {
            if(this.LiteDatabase != null) {
                this.LiteDatabase.Dispose();
            }
        }
    }
}