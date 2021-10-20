using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace PixelGraph.Rendering.Utilities
{
    public static class ResourceLoader
    {
        public static Stream Open(string filePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream(filePath);
        }

        public static MemoryStream Buffer(string filePath)
        {
            using var stream = Open(filePath);
            if (stream == null) throw new ApplicationException($"Unable to locate resource '{filePath}'!");

            var buffer = new MemoryStream();

            try {
                stream.CopyTo(buffer);
                return buffer;
            }
            catch {
                buffer.Dispose();
                throw;
            }
        }

        public static async Task<MemoryStream> BufferAsync(string filePath)
        {
            await using var stream = Open(filePath);
            if (stream == null) throw new ApplicationException($"Unable to locate resource '{filePath}'!");

            var buffer = new MemoryStream();

            try {
                await stream.CopyToAsync(buffer);
                return buffer;
            }
            catch {
                await buffer.DisposeAsync();
                throw;
            }
        }

        public static byte[] GetBytes(string filePath)
        {
            using var stream = Buffer(filePath);
            return stream.GetBuffer();
        }

        public static async Task<byte[]> GetBytesAsync(string filePath)
        {
            await using var stream = await BufferAsync(filePath);
            return stream.GetBuffer();
        }

        //public static byte[] GetShaderByteCode(string name)
        //{
        //    return GetBytes($"{compiledShaderPath}.{name}.cso");
        //}
    }
}
