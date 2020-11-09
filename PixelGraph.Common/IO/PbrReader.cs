using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO
{
    public interface IPbrReader
    {
        Task<PbrProperties[]> LoadAsync(string localFile, CancellationToken token = default);
        Task<PbrProperties> LoadGlobalAsync(string localFile, CancellationToken token = default);
        Task<PbrProperties> LoadLocalAsync(string localFile, CancellationToken token = default);
        bool TryExpandRange(PbrProperties texture, out PbrProperties[] results);
    }

    internal class PbrReader : PropertySerializer, IPbrReader
    {
        private readonly IInputReader reader;


        public PbrReader(IInputReader reader)
        {
            this.reader = reader;
        }

        public async Task<PbrProperties[]> LoadAsync(string localFile, CancellationToken token = default)
        {
            var global = !string.Equals(localFile, "pbr.properties", StringComparison.InvariantCultureIgnoreCase);

            if (global) {
                var texture = await LoadGlobalAsync(localFile, token);

                return TryExpandRange(texture, out var subTextures)
                    ? subTextures : new []{texture};
            }
            else {
                var texture = await LoadLocalAsync(localFile, token);
                return new []{texture};
            }
        }

        public async Task<PbrProperties> LoadGlobalAsync(string localFile, CancellationToken token = default)
        {
            var name = Path.GetFileName(localFile);
            name = name[..^15];

            var properties = new PbrProperties {
                FileName = localFile,
                Name = name,
                Path = Path.GetDirectoryName(localFile),
                UseGlobalMatching = true,
            };

            await PopulateAsync(properties, localFile, token);
            return properties;
        }

        public async Task<PbrProperties> LoadLocalAsync(string localFile, CancellationToken token = default)
        {
            var itemPath = Path.GetDirectoryName(localFile);

            var properties = new PbrProperties {
                FileName = localFile,
                Name = Path.GetFileName(itemPath),
                Path = Path.GetDirectoryName(itemPath),
            };

            await PopulateAsync(properties, localFile, token);
            return properties;
        }

        public bool TryExpandRange(PbrProperties texture, out PbrProperties[] results)
        {
            if (!texture.RangeMin.HasValue || !texture.RangeMax.HasValue) {
                results = null;
                return false;
            }

            // clone texture for each index in range
            var min = texture.RangeMin.Value;
            var max = texture.RangeMax.Value;

            var resultList = new List<PbrProperties>();
            for (var i = min; i <= max; i++) {
                var subTexture = texture.Clone();
                subTexture.Name = i.ToString();
                subTexture.Alias = texture.Name;
                resultList.Add(subTexture);
            }

            results = resultList.ToArray();
            return true;
        }

        private async Task PopulateAsync(PbrProperties texture, string filename, CancellationToken token)
        {
            await using var stream = reader.Open(filename);
            await ReadAsync(stream, texture, token);
        }
    }
}
