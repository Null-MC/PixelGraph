using System;

namespace McPbrPipeline.Internal.Extensions
{
    internal static class ConsoleEx
    {
        private static readonly object handle = new object();


        public static void Write(string text, ConsoleColor color)
        {
            ColorWrap(() => Console.Write(text), color);
        }

        public static void WriteLine(string text, ConsoleColor color)
        {
            ColorWrap(() => Console.WriteLine(text), color);
        }

        public static void WriteLine()
        {
            lock (handle) {
                Console.WriteLine();
            }
        }

        private static void ColorWrap(Action action, ConsoleColor color)
        {
            lock (handle) {
                Console.ForegroundColor = color;
                action();
                Console.ResetColor();
            }
        }
    }
}
