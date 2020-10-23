using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LiteDB;

namespace LiteDbFlex
{    
    public class LitedbFlexBuilder<T> : IDisposable
    where T : class
    {
        public ILiteDatabase LiteDatabase {get; private set;}
        public ILiteCollection<T> LiteCollection {get; private set;}
        public bool IsTran {get; private set;}
        public bool IsTraned {get; private set;}
        public object Result {get; private set;}

        private string tableName;

        public LitedbFlexBuilder(string additionalName = "") {
            this.LiteDatabase = LiteDbResolver.Resolve<T>(additionalName);
            tableName = typeof(T).GetAttributeValue((LiteDbTableAttribute tableAttribute) => tableAttribute.TableName);
            this.LiteCollection = this.LiteDatabase.GetCollection<T>(tableName);            
        }

        public LitedbFlexBuilder<T> BeginTrans() {
            this.IsTran = this.LiteDatabase.BeginTrans();
            return this;
        }

        public LitedbFlexBuilder<T> Commit() {
            if(this.IsTran) {
                this.IsTraned = this.LiteDatabase.Commit();
            }
            return this;
        }

        public LitedbFlexBuilder<T> Rollback() {
            if(this.IsTran) {
                this.LiteDatabase.Rollback();
            }
            return this;
        }        

        public LitedbFlexBuilder<T> EnsureIndex<K>(Expression<Func<T, K>> keySelector, bool unique = false)
        {
            this.LiteCollection.EnsureIndex(keySelector, unique);
            return this;
        }

        public LitedbFlexBuilder<T> DropIndex(string name) {
            this.LiteCollection.DropIndex(name);
            return this;
        }

        public LitedbFlexBuilder<T> Gets() {
            this.Result = this.LiteCollection.FindAll().ToList<T>();
            return this;
        }

        public LitedbFlexBuilder<T> Gets(Expression<Func<T, bool>> predicate, int skip = 0, int limit = int.MaxValue) {
            if(predicate != null)
                this.Result = this.LiteCollection.Find(predicate, skip, limit).ToList<T>();
            else 
                this.Result = this.LiteCollection.FindAll().Skip(skip).Take(limit).ToList<T>();
            return this;
        }

        public IEnumerable<T> GetEnumerable(Expression<Func<T, bool>> predicate) {
            return this.LiteCollection.Find(predicate);
        }

        public LitedbFlexBuilder<T> Get(int id) {
            this.Result = this.LiteCollection.FindById(id);
            return this;
        }

        public LitedbFlexBuilder<T> Get(Expression<Func<T, bool>> predicate) {
            this.Result = this.LiteCollection.FindOne(predicate);
            return this;
        }

        public LitedbFlexBuilder<T> Insert(T entity) {
            this.Result = this.LiteCollection.Insert(entity);
            return this;
        }

        public LitedbFlexBuilder<T> Insert(IEnumerable<T> entities, int batchSize = 5000) {
            this.Result = this.LiteCollection.InsertBulk(entities, batchSize);
            return this;
        }

        public LitedbFlexBuilder<T> Update(T entity) {
            this.Result = this.LiteCollection.Update(entity);
            return this;
        }

        public LitedbFlexBuilder<T> Update(Expression<Func<T, T>> expression, Expression<Func<T, bool>> predicate) {
            this.Result = this.LiteCollection.UpdateMany(expression, predicate);
            return this;
        }

        public LitedbFlexBuilder<T> Delete(int id) {
            this.Result = this.LiteCollection.Delete(id);
            return this;
        }

        public LitedbFlexBuilder<T> Delete(Expression<Func<T, bool>> predicate) {
            this.Result = this.LiteCollection.DeleteMany(predicate);
            return this;
        }

        public LitedbFlexBuilder<T> Delete() {
            this.Result = this.LiteCollection.DeleteAll();
            return this;            
        }

        public int GetIntResult() {
            return (int)this.Result;
        }

        public bool GetIsResult() {
            return (bool)this.Result;            
        }

        public TResult GetResult<TResult>() 
        where TResult : class {
            return (TResult)this.Result;
        }

        public void Dispose() {
            if(this.IsTran && !this.IsTraned) {
                this.LiteDatabase.Commit();
            }

            if(this.LiteDatabase != null) {
                this.LiteDatabase.Dispose();
            }
        }
    }
}