using PixelGraph.Common.Extensions;
using System.Collections.Generic;
using System.IO;

namespace PixelGraph.Common.IO.Publishing
{
    public interface IPublisherMapping
    {
        //IDictionary<string, string> Mappings {get; set;}

        bool TryMap(string sourceFile, out string destFile);
        bool TryMap(string sourcePath, string sourceName, out string destPath, out string destName);
    }

    internal class PublisherMappingBase : IPublisherMapping
    {
        private readonly IDictionary<string, string> map;
        //public IDictionary<string, string> Mappings {get; set;}


        public PublisherMappingBase(IDictionary<string, string> map)
        {
            this.map = map;
        }

        public virtual bool TryMap(string sourceFile, out string destFile)
        {
            return map.TryGetValue(sourceFile, out destFile);
        }

        public virtual bool TryMap(string sourcePath, string sourceName, out string destPath, out string destName)
        {
            var sourceFile = PathEx.Join(sourcePath, sourceName);
            sourceFile = PathEx.Normalize(sourceFile);

            if (!map.TryGetValue(sourceFile, out var destFile)) {
                destName = null;
                destPath = null;
                return false;
            }

            destName = Path.GetFileName(destFile);
            destPath = Path.GetDirectoryName(destFile);
            return true;
        }
    }
}
