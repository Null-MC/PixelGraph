using McPbrPipeline.Internal.Input;
using System;

namespace McPbrPipeline.Internal
{
    internal class PackProperties : PropertiesFile
    {
        public string Source {get; set;}
        public DateTime WriteTime {get; set;}

        public int PackFormat => Get<int>("pack.format");
        public string PackDescription => Get<string>("pack.description");
        public string PackTags => Get<string>("pack.tags");

        public string Sampler => Get<string>("sampler");
        public int? TextureSize => Get<int?>("texture.size");
        public float? TextureScale => Get<float?>("texture.scale");
        public string InputFormat => Get<string>("input.format");
        public string OutputFormat => Get<string>("output.format");
        public string OutputEncoding => Get("output.encoding", "png");
    }
}
