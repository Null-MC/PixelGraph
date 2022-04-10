using System;
using System.Collections.Generic;
using System.Linq;

namespace PixelGraph.Common.IO
{
    public static class UnitHelper
    {
        private static readonly string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB

        private static readonly SortedList<long, string> cutoff = new() {
            {59, "{3:S}"},
            {60, "{2:M}"},
            {60*60-1, "{2:M}, {3:S}"},
            {60*60, "{1:H}"},
            {24*60*60-1, "{1:H}, {2:M}"},
            {24*60*60, "{0:D}"},
            {long.MaxValue , "{0:D}, {1:H}"},
        };


        public static string GetReadableSize(long byteCount)
        {
            if (byteCount == 0) return $"0{suf[0]}";

            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return $"{Math.Sign(byteCount) * num}{suf[place]}";
        }

        public static string GetReadableTimespan(TimeSpan ts)
        {
            var find = cutoff.Keys.ToList().BinarySearch((long)ts.TotalSeconds);

            var near = find < 0 ? Math.Abs(find) - 1 : find;

            return string.Format(new HMSFormatter(), cutoff[cutoff.Keys[near]],
                ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
        }

        private class HMSFormatter : ICustomFormatter, IFormatProvider
        {
            // list of Formats, with a P customformat for pluralization
            private static readonly Dictionary<string, string> timeFormats = new() {
                {"S", "{0:P:Seconds:Second}"},
                {"M", "{0:P:Minutes:Minute}"},
                {"H","{0:P:Hours:Hour}"},
                {"D", "{0:P:Days:Day}"}
            };

            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                return string.Format(new PluralFormatter(), timeFormats[format], arg);
            }

            public object GetFormat(Type formatType)
            {
                return formatType == typeof(ICustomFormatter) ? this : null;
            }
        }

        private class PluralFormatter : ICustomFormatter, IFormatProvider
        {
            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                if (arg != null) {
                    var parts = format.Split(':'); // ["P", "Plural", "Singular"]

                    if (parts[0] == "P") {
                        var partIndex = arg.ToString() == "1" ? 2 : 1;
                        var part = parts.Length > partIndex ? parts[partIndex] : string.Empty;
                        return $"{arg} {part}";
                    }
                }

                return string.Format(format, arg);
            }

            public object GetFormat(Type formatType)
            {
                return formatType == typeof(ICustomFormatter) ? this : null;
            }
        }
    }
}
