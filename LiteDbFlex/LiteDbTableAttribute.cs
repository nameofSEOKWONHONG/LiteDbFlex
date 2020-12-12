using System;
using System.Collections.Generic;

namespace LiteDbFlex {

    /// <summary>
    ///     litedb entity extension attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class LiteDbTableAttribute : Attribute {

        public LiteDbTableAttribute(string fileName, string tableName, string[] indexNames = null, bool[] indexUniques = null) {
            FileName = fileName;
            TableName = tableName;
            if(indexNames != null) {
                for (var i = 0; i < indexNames.Length; i++) {
                    var unique = true;
                    if (indexUniques != null) unique = indexUniques[i];
                    Indexes.Add(indexNames[i], unique);
                }
            }
        }

        public string FileName { get; }
        public string TableName { get; }
        public readonly Dictionary<string, bool> Indexes = new Dictionary<string, bool>();
    }
}