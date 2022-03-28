using MinecraftMappings.Minecraft.LegacyJavaToBedrockMappings;
using PixelGraph.Common.Extensions;
using System;
using System.Collections.Generic;

namespace PixelGraph.Common.IO.Publishing
{
    public interface IJavaToBedrockPublishMapping : IPublisherMapping {}

    internal class JavaToBedrockPublishMapping : PublisherMappingBase, IJavaToBedrockPublishMapping
    {
        protected override IDictionary<string, string> OnBuildMappings()
        {
            var data = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            data.Merge(LegacyBlockMappings.Instance);
            data.Merge(LegacyEntityMappings.Instance);
            data.Merge(LegacyItemMappings.Instance);
            data.Merge(LegacyOtherMappings.Instance);
            return data;
        }
    }
}
