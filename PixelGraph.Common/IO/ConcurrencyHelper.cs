using System;

namespace PixelGraph.Common.IO;

public static class ConcurrencyHelper
{
    public static int GetDefaultValue()
    {
        var count = Environment.ProcessorCount;
        if (count < 1) return 1;

        return (int)(count * 0.5f + 0.5f);
    }
}