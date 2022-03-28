using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO.Publishing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PixelGraph.Common.IO.Importing
{
    internal class BedrockToJavaImportMapping : PublisherMappingBase
    {
        protected override IDictionary<string, string> OnBuildMappings()
        {
            var materialMap = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var (key, value) in JavaToBedrockPublishMapping.materialMap)
                materialMap[value] = key;

            return materialMap;
        }

        public override bool Contains(string sourceFile)
        {
            return Mappings.Keys.Any(sourceFile.EndsWith);
        }

        public override bool TryMap(string sourcePath, string sourceName, out string destPath, out string destName)
        {
            var sourceFile = PathEx.Join(sourcePath, sourceName);
            sourceFile = PathEx.Normalize(sourceFile);

            foreach (var mapping in Mappings) {
                if (!sourceFile.EndsWith(mapping.Key)) continue;

                destName = Path.GetFileName(mapping.Value);
                destPath = Path.GetDirectoryName(mapping.Value);
                return true;
            }

            destName = null;
            destPath = null;
            return false;
        }
    }
}
