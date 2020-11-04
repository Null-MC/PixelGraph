using PixelGraph.Common.IO;
using System;

namespace PixelGraph.Common
{
    public class PackProperties : PropertiesFile
    {
        public string Source {get; set;}
        public DateTime WriteTime {get; set;}

        public string PackEdition => Get("pack.edition", "java");
        public int PackFormat => Get("pack.format", 5);
        public string PackDescription => Get<string>("pack.description");
        public string PackTags => Get<string>("pack.tags");

        public string Sampler => Get<string>("sampler");
        public int? TextureSize => Get<int?>("texture.size");
        public float? TextureScale => Get<float?>("texture.scale");
        public string InputFormat => Get<string>("input.format");
        public string OutputFormat => Get<string>("output.format");
        public string OutputEncoding => Get("output.encoding", ImageExtensions.Default);
    }
}
