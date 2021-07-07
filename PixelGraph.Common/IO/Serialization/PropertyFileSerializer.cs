using PixelGraph.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO.Serialization
{
    //public interface IPropertyWriter<T>
    //    where T : new()
    //{
    //    Task<T> ReadAsync(StreamReader reader, CancellationToken token = default);
    //    Task WriteAsync(StreamWriter writer, T properties, CancellationToken token = default);
    //}

    internal class PropertyFileSerializer<T> //: IPropertyWriter<T>
        where T : new()
    {
        private readonly Dictionary<string, PropertyInfo> propertyMap;


        public PropertyFileSerializer()
        {
            propertyMap = new Dictionary<string, PropertyInfo>(StringComparer.InvariantCultureIgnoreCase);

            MapTypeProperties();
        }

        public async Task<T> ReadAsync(StreamReader reader, CancellationToken token = default)
        {
            var data = new T();

            string line;
            while ((line = await reader.ReadLineAsync()) != null) {
                token.ThrowIfCancellationRequested();

                line = line.Trim();
                if (line.StartsWith('#') || string.IsNullOrEmpty(line)) continue;

                if (!TryParseLine(line, out var propertyName, out var value))
                    throw new ApplicationException("Failed to parse property line '{line}'!");

                SetProperty(data, propertyName, value);
            }

            return data;
        }

        public async Task WriteAsync(StreamWriter writer, T properties, CancellationToken token = default)
        {
            foreach (var property in propertyMap.Values) {
                var valueObj = property.GetValue(properties);
                if (valueObj == null) return;

                var valueStr = valueObj as string ?? valueObj.ToString();

                await writer.WriteAsync(property.Name);
                await writer.WriteAsync(" = ");
                await writer.WriteLineAsync(valueStr);
            }
        }

        private void MapTypeProperties()
        {
            var propertyList = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.IgnoreCase);

            foreach (var property in propertyList)
                propertyMap[property.Name] = property;
        }

        private void SetProperty(T obj, string propertyName, string value)
        {
            if (!propertyMap.TryGetValue(propertyName, out var property))
                throw new ApplicationException($"Property '{propertyName}' not found on type '{typeof(T).Name}'!");

            property.SetValue(obj, value.To(property.PropertyType));
        }

        private static bool TryParseLine(string line, out string propertyName, out string value)
        {
            var i = line.IndexOf('=');

            if (i < 0) {
                propertyName = value = null;
                return false;
            }

            propertyName = line[..i].TrimEnd();
            value = line[(i+1)..].TrimStart();
            return true;
        }
    }
}
