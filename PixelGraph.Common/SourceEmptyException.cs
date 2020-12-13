using System;

namespace PixelGraph.Common
{
    public class SourceEmptyException : Exception
    {
        public SourceEmptyException(string message) : base(message) {}
    }

    public class HeightSourceEmptyException : SourceEmptyException
    {
        public HeightSourceEmptyException(string message) : base(message) {}

        public HeightSourceEmptyException() : base("No height sources found!") {}
    }
}
