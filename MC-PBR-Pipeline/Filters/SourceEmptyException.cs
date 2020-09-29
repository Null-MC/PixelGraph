using System;

namespace McPbrPipeline.Filters
{
    public class SourceEmptyException : Exception
    {
        public SourceEmptyException(string message) : base(message) {}
    }
}
