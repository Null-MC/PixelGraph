using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Textures
{
    internal interface IPbrProperties
    {
        string Name {get;}
        string Path {get;}
        bool UseGlobalMatching {get;}

        T Get<T>(string key, T defaultValue = default);

        bool Wrap => Get(PbrProperty.Wrap, true);
        bool ResizeEnabled => Get(PbrProperty.ResizeEnabled, true);

        string AlbedoTexture => Get<string>(PbrProperty.AlbedoTexture);

        string HeightTexture => Get<string>(PbrProperty.HeightTexture);
        float HeightScale => Get(PbrProperty.HeightScale, 1f);

        string NormalTexture => Get<string>(PbrProperty.NormalTexture);
        bool NormalFromHeight => Get(PbrProperty.NormalFromHeight, true);
        float NormalStrength => Get(PbrProperty.NormalStrength, 1f);
        float NormalDepthScale => Get(PbrProperty.NormalDepthScale, 1f);

        string SpecularTexture => Get<string>(PbrProperty.SpecularTexture);
        string SpecularColor => Get<string>(PbrProperty.SpecularColor);
        float SmoothScale => Get(PbrProperty.SmoothScale, 1f);
        float RoughScale => Get(PbrProperty.RoughScale, 1f);
        float MetalScale => Get(PbrProperty.MetalScale, 1f);
        float EmissiveScale => Get(PbrProperty.EmissiveScale, 1f);

        string EmissiveTexture => Get<string>(PbrProperty.EmissiveTexture);
    }

    internal class PbrProperties : IPbrProperties
    {
        private readonly Dictionary<string, string> properties;

        public string Name {get; set;}
        public string Path {get; set;}
        public bool UseGlobalMatching {get; set;}


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
    }
}
