using PixelGraph.Common.ConnectedTextures;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PixelGraph.Common.IO.Serialization
{
    public interface IMaterialReader
    {
        Task<MaterialProperties> LoadAsync(string localFile, CancellationToken token = default);
        Task<MaterialProperties> LoadGlobalAsync(string localFile, CancellationToken token = default);
        Task<MaterialProperties> LoadLocalAsync(string localFile, CancellationToken token = default);
        bool TryExpandRange(MaterialProperties texture, out MaterialProperties[] results);
    }

    internal class MaterialReader : IMaterialReader
    {
        private readonly IInputReader reader;
        private readonly IDeserializer deserializer;


        public MaterialReader(IInputReader reader)
        {
            this.reader = reader;

            deserializer = new DeserializerBuilder()
                .WithTypeConverter(new YamlStringEnumConverter())
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build();
        }

        public Task<MaterialProperties> LoadAsync(string localFile, CancellationToken token = default)
        {
            var name = Path.GetFileName(localFile);
            var global = !(string.Equals(name, "pbr.yml", StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(name, "mat.yml", StringComparison.InvariantCultureIgnoreCase));
            return global ? LoadGlobalAsync(localFile, token) : LoadLocalAsync(localFile, token);
        }

        public async Task<MaterialProperties> LoadGlobalAsync(string localFile, CancellationToken token = default)
        {
            if (localFile == null) throw new ArgumentNullException(nameof(localFile));

            var material = await ParseDocumentAsync(localFile);

            material.UseGlobalMatching = true;
            material.LocalFilename = localFile;
            material.LocalPath = Path.GetDirectoryName(localFile);

            if (string.IsNullOrWhiteSpace(material.Name)) {
                material.Name = Path.GetFileName(localFile);

                if (material.Name.EndsWith(".mat.yml", StringComparison.InvariantCultureIgnoreCase))
                    material.Name = material.Name[..^8];

                if (material.Name.EndsWith(".pbr.yml", StringComparison.InvariantCultureIgnoreCase))
                    material.Name = material.Name[..^8];
            }

            Upgrade(material);

            return material;
        }

        public async Task<MaterialProperties> LoadLocalAsync(string localFile, CancellationToken token = default)
        {
            var itemPath = Path.GetDirectoryName(localFile);
            if (!reader.FileExists(localFile)) return null;

            var material = await ParseDocumentAsync(localFile);
            material.LocalFilename = localFile;
            material.LocalPath = Path.GetDirectoryName(itemPath);

            if (string.IsNullOrWhiteSpace(material.Name))
                material.Name = Path.GetFileName(itemPath);

            Upgrade(material);

            return material;
        }

        public bool TryExpandRange(MaterialProperties material, out MaterialProperties[] results)
        {
            if (!material.RangeMin.HasValue || !material.RangeMax.HasValue) {
                results = null;
                return false;
            }

            // clone texture for each index in range
            var min = material.RangeMin.Value;
            var max = material.RangeMax.Value;

            var resultList = new List<MaterialProperties>();
            for (var i = min; i <= max; i++) {
                var expandedMaterial = material.Clone();
                expandedMaterial.Name = i.ToString();
                expandedMaterial.Alias = material.Name;
                resultList.Add(expandedMaterial);
            }

            results = resultList.ToArray();
            return true;
        }

        /// <summary>
        /// Migrates material properties from deprecated fields to their new locations.
        /// </summary>
        private void Upgrade(MaterialProperties material)
        {
            if (material.Color?.__PreviewTint != null && material.TintColor == null)
                material.TintColor = material.Color?.__PreviewTint;

            if (material.Color?.Value == null && ((material.Color?.__ValueRed.HasValue ?? false) || (material.Color?.__ValueRed.HasValue ?? false) || (material.Color?.__ValueRed.HasValue ?? false))) {
                var red = (byte?)material.Color?.__ValueRed ?? 0;
                var green = (byte?)material.Color?.__ValueGreen ?? 0;
                var blue = (byte?)material.Color?.__ValueBlue ?? 0;

                material.Color.Value = $"#{red:X2}{green:X2}{blue:X2}";
            }
        }

        private async Task<MaterialProperties> ParseDocumentAsync(string localFile)
        {
            await using var stream = reader.Open(localFile);
            using var streamReader = new StreamReader(stream);
            var document = deserializer.Deserialize<MaterialProperties>(streamReader)
                ?? new MaterialProperties();

            if (document.CTM?.Method != null && parseMap.TryGetValue(document.CTM.Method, out var replaceMethod))
                document.CTM.Method = replaceMethod;

            return document;
        }

        /// <summary> For deprecated mappings </summary>
        private static readonly Dictionary<string, string> parseMap = new(StringComparer.InvariantCultureIgnoreCase) {
            ["full"] = CtmTypes.Optifine_Full,
            ["compact"] = CtmTypes.Optifine_Compact,
            ["horizontal"] = CtmTypes.Optifine_Horizontal,
            ["vertical"] = CtmTypes.Optifine_Vertical,
            ["top"] = CtmTypes.Optifine_Top,
            ["random"] = CtmTypes.Optifine_Random,
            ["repeat"] = CtmTypes.Optifine_Repeat,
            ["fixed"] = CtmTypes.Optifine_Fixed,
        };
    }
}
