using System;

namespace PixelGraph.UI.Internal.Utilities
{
    internal static class BoxCast
    {
        public static T To<T>(this object value)
        {
            if (value == null) return default;

            var nullType = Nullable.GetUnderlyingType(typeof(T));
            var t = nullType ?? typeof(T);

            if (t.IsEnum) {
                var stringValue = value as string ?? value.ToString();
                if (string.IsNullOrWhiteSpace(stringValue)) return default;

                return (T)Enum.Parse(t, stringValue, true);
            }

            return (T)Convert.ChangeType(value, t);
        }
    }
}
