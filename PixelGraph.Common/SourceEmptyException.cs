using System;

namespace PixelGraph.Common
{
    public class SourceEmptyException : Exception
    {
        public SourceEmptyException(string message) : base(message) {}
    }
}
