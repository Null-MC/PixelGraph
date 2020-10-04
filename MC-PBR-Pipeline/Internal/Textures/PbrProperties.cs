using McPbrPipeline.Internal.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SysPath = System.IO.Path;

namespace McPbrPipeline.Internal.Textures
{
    internal class PbrProperties
    {
        private readonly Dictionary<string, string> properties;

        public string Name {get; set;}
        public string Path {get; set;}
        public bool UseGlobalMatching {get; set;}

        public bool Wrap => Get(PbrProperty.Wrap, true);
        public bool ResizeEnabled => Get(PbrProperty.ResizeEnabled, true);

        public string AlbedoTexture => Get<string>(PbrProperty.AlbedoTexture);

        public string HeightTexture => Get<string>(PbrProperty.HeightTexture);
        public float HeightScale => Get(PbrProperty.HeightScale, 1f);

        public string NormalTexture => Get<string>(PbrProperty.NormalTexture);
        public bool NormalFromHeight => Get(PbrProperty.NormalFromHeight, true);
        public float NormalStrength => Get(PbrProperty.NormalStrength, 1f);
        public float NormalDepthScale => Get(PbrProperty.NormalDepthScale, 1f);

        public string SpecularTexture => Get<string>(PbrProperty.SpecularTexture);
        public string SpecularColor => Get<string>(PbrProperty.SpecularColor);
        public float SmoothScale => Get(PbrProperty.SmoothScale, 1f);
        public float RoughScale => Get(PbrProperty.RoughScale, 1f);
        public float MetalScale => Get(PbrProperty.MetalScale, 1f);
        public float EmissiveScale => Get(PbrProperty.EmissiveScale, 1f);

        public string EmissiveTexture => Get<string>(PbrProperty.EmissiveTexture);


        public PbrProperties()
        {
            properties = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            if (!properties.TryGetValue(key, out var stringValue)) return defaultValue;
            if (string.IsNullOrEmpty(stringValue)) return defaultValue;

            var valueType = typeof(T);
            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>))
                valueType = Nullable.GetUnderlyingType(valueType) ?? valueType;

            return (T)Convert.ChangeType(stringValue, valueType);
        }

        public async Task ReadAsync(Stream stream, CancellationToken token = default)
        {
            using var reader = new StreamReader(stream, null, true, -1, true);

            string line;
            while ((line = await reader.ReadLineAsync()) != null) {
                line = line.Trim();
                if (line.StartsWith('#') || string.IsNullOrEmpty(line)) continue;

                var i = line.IndexOf('=');
                if (i < 0) continue;

                var key = line[..i].TrimEnd();
                var value = line[(i+1)..].TrimStart();
                properties[key] = value;
            }
        }

        public IEnumerable<string> GetAllTextures(IInputReader reader)
        {
            return TextureTags.All
                .Select(tag => GetTextureFile(reader, tag))
                .Where(file => file != null).Distinct();
        }

        public string GetTextureFile(IInputReader reader, string type)
        {
            var filename = TextureTags.Get(this, type);

            while (filename != null) {
                var linkedFilename = TextureTags.Get(this, filename);
                if (string.IsNullOrEmpty(linkedFilename)) break;

                type = filename;
                filename = linkedFilename;
            }

            var srcPath = UseGlobalMatching
                ? Path : SysPath.Combine(Path, Name);

            if (!string.IsNullOrEmpty(filename)) {
                return SysPath.Combine(srcPath, filename);
            }

            var matchName = TextureTags.GetMatchName(this, type);

            return reader.EnumerateFiles(srcPath, matchName).FirstOrDefault(f => {
                var ext = SysPath.GetExtension(f);
                return ImageExtensions.Supported.Contains(ext, StringComparer.InvariantCultureIgnoreCase);
            });
        }
    }
}
