using System;
using System.Collections.Generic;

namespace PixelGraph.Common.Extensions
{
    public static class ExceptionExtensions
    {
        public static IEnumerable<string> UnfoldMessages(this Exception error)
        {
            var e = error;
            while (e != null) {
                yield return e.Message;
                e = e.InnerException;
            }
        }

        public static string UnfoldMessageString(this Exception error)
        {
            return string.Join(" ", UnfoldMessages(error));
        }
    }
}
