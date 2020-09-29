using System;

namespace McPbrPipeline.Internal.Filtering
{
    public class SourceEmptyException : Exception
    {
        public SourceEmptyException(string message) : base(message) {}
    }
}
