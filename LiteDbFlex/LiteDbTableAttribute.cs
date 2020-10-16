using System;
using System.Collections.Generic;
using System.Text;

namespace LiteDbFlex {
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class LiteDbTableAttribute : Attribute  {
        public string FileName { get; private set; }
        public string TableName { get; private set; }
        public LiteDbTableAttribute(string fileName, string tableName) {
            this.FileName = fileName;
            this.TableName = tableName;
        }
    }
}
