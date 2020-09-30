using System;

namespace McPbrPipeline.Internal
{
    internal static class ConsoleEx
    {
        public static void Write(string text, ConsoleColor color)
        {
            ColorWrap(() => Console.Write(text), color);
        }

        public static void WriteLine(string text, ConsoleColor color)
        {
            ColorWrap(() => Console.WriteLine(text), color);
        }

        private static void ColorWrap(Action action, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            action();
            Console.ResetColor();
        }
    }
}
