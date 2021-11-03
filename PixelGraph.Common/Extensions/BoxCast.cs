using System;

namespace PixelGraph.Common.Extensions
{
    public static class BoxCast
    {
        public static object To(this object value, Type targetType)
        {
            if (value == null) return default;

            var nullType = Nullable.GetUnderlyingType(targetType);
            var t = nullType ?? targetType;

            if (t.IsEnum) {
                var stringValue = value as string ?? value.ToString();
                if (string.IsNullOrWhiteSpace(stringValue)) return default;

                return Enum.Parse(t, stringValue, true);
            }

            //if (value is Color colorValue && targetType == typeof(string)) {
            //    return  colorValue as string;
            //}

            return Convert.ChangeType(value, t);
        }

        public static T To<T>(this object value)
        {
            return (T)To(value, typeof(T));
        }
    }
}
