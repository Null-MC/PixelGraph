using PixelGraph.Common.IO.Publishing;
using System;
using System.Collections.Generic;

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
    }
}
