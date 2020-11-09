using System;
using System.Collections.Generic;

namespace PixelGraph.Common.IO
{
    public abstract class PropertiesFile
    {
        public Dictionary<string, string> Properties {get; protected set;}


        protected PropertiesFile()
        {
            Properties = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            if (!Properties.TryGetValue(key, out var stringValue)) return defaultValue;
            if (string.IsNullOrEmpty(stringValue)) return defaultValue;

            var valueType = typeof(T);
            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>))
                valueType = Nullable.GetUnderlyingType(valueType) ?? valueType;

            return (T)Convert.ChangeType(stringValue, valueType);
        }

        public bool TrySet(string property)
        {
            var i = property.IndexOf('=');
            if (i < 0) return false;

            var key = property[..i].TrimEnd();
            var value = property[(i+1)..].TrimStart();
            Properties[key] = value;
            return true;
        }
    }
}
