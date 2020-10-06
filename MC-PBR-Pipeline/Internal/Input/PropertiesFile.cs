using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Input
{
    internal class PropertiesFile
    {
        protected readonly Dictionary<string, string> Properties;


        public PropertiesFile()
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

                var i = line.IndexOf('=');
                if (i < 0) continue;

                var key = line[..i].TrimEnd();
                var value = line[(i+1)..].TrimStart();
                Properties[key] = value;
            }
        }
    }
}
