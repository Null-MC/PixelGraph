using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PixelGraph.Common.IO
{
    public abstract class PropertiesFile : INotifyPropertyChanged
    {
        public Dictionary<string, string> Properties {get; protected set;}


        protected PropertiesFile()
        {
            Properties = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
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

        protected T Get<T>(string key, T defaultValue = default)
        {
            if (!Properties.TryGetValue(key, out var stringValue)) return defaultValue;
            if (string.IsNullOrEmpty(stringValue)) return defaultValue;

            var valueType = typeof(T);
            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>))
                valueType = Nullable.GetUnderlyingType(valueType) ?? valueType;

            return (T)Convert.ChangeType(stringValue, valueType);
        }

        protected void Set(string key, object value)
        {
            Properties[key] = value as string ?? value?.ToString();
            OnPropertyChanged($"Properties[{key}]");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
