using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Input
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

        public async Task ReadAsync(Stream stream, CancellationToken token = default)
        {
            using var reader = new StreamReader(stream, null, true, -1, true);

            string line;
            while ((line = await reader.ReadLineAsync()) != null) {
                line = line.Trim();
                if (line.StartsWith('#') || string.IsNullOrEmpty(line)) continue;

                TrySet(line);
            }
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
