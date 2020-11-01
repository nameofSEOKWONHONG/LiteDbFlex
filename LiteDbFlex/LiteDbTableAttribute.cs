using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace LiteDbFlex {
    /// <summary>
    /// litedb entity extension attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class LiteDbTableAttribute : Attribute  {
        public string FileName { get; private set; }
        public string TableName { get; private set; }
        public LiteDbTableAttribute(string fileName, string tableName) {
            this.FileName = fileName;
            this.TableName = tableName;
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class LiteDbIndexAttribute : Attribute {
        public Dictionary<string, bool> Indexes = new Dictionary<string, bool>();
        public LiteDbIndexAttribute(string[] indexNames, bool[] indexUniques = null) {
            for(var i=0; i<indexNames.Length; i++) {
                var unique = true;
                if (indexUniques != null) {
                    unique = indexUniques[i];
                }
                Indexes.Add(indexNames[i], unique);
            }
        }
    }
}
