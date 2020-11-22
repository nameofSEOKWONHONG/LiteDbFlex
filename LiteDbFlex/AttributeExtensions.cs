using System;
using System.Linq;

namespace LiteDbFlex {

    /// <summary>
    ///     get value of attrivute extension
    /// </summary>
    internal static class AttributeExtensions {

        public static TValue GetAttributeValue<TAttribute, TValue>(
            this Type type,
            Func<TAttribute, TValue> valueSelector)
            where TAttribute : Attribute {
            var att = type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
            if (att != null) return valueSelector(att);
            return default;
        }
    }
}