using PixelGraph.Common.Extensions;
using System.Collections.Generic;
using System.IO;

namespace PixelGraph.Common.IO.Publishing
{
    public interface IPublisherMapping
    {
        IDictionary<string, string> Mappings {get;}

        bool TryMap(string sourceFile, out string destFile);
        bool TryMap(string sourcePath, string sourceName, out string destPath, out string destName);
    }

    internal class PublisherMappingBase : IPublisherMapping
    {
        public IDictionary<string, string> Mappings {get;}
        //public IDictionary<string, string> Mappings {get; set;}


        public PublisherMappingBase(IDictionary<string, string> map)
        {
            Mappings = map;
        }

        public virtual bool TryMap(string sourceFile, out string destFile)
        {
            return Mappings.TryGetValue(sourceFile, out destFile);
        }

        public virtual bool TryMap(string sourcePath, string sourceName, out string destPath, out string destName)
        {
            var sourceFile = PathEx.Join(sourcePath, sourceName);
            sourceFile = PathEx.Normalize(sourceFile);

            if (!Mappings.TryGetValue(sourceFile, out var destFile)) {
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
