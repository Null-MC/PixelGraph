using PixelGraph.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    internal class ObjectPropertyFileSerializer<T> : PropertyFileSerializer //: IPropertyWriter<T>
        where T : new()
    {
        private readonly Dictionary<string, PropertyInfo> propertyMap;


        public ObjectPropertyFileSerializer()
        {
            propertyMap = new Dictionary<string, PropertyInfo>(StringComparer.InvariantCultureIgnoreCase);

            MapTypeProperties();
        }

        public new async Task<T> ReadAsync(StreamReader reader, CancellationToken token = default)
        {
            var data = new T();

            await foreach (var (propertyName, propertyValue) in base.ReadAsync(reader, token))
                SetProperty(data, propertyName, propertyValue);

            return data;
        }

        public async Task WriteAsync(StreamWriter writer, T properties, CancellationToken token = default)
        {
            await WriteAsync(writer, propertyMap.Values.Select(property => {
                var valueObj = property.GetValue(properties);
                //if (valueObj == null) return null;

                var valueStr = valueObj as string ?? valueObj?.ToString();
                return new KeyValuePair<string, string>(property.Name, valueStr);
            }), token);
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
