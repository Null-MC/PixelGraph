using System;

namespace McPbrPipeline.Internal.Publishing
{
    public class SourceEmptyException : Exception
    {
        public SourceEmptyException(string message) : base(message) {}
    }
}
