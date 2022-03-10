using Microsoft.Extensions.DependencyInjection;
using MinecraftMappings.Internal.Textures.Block;
using MinecraftMappings.Minecraft;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Tests.Internal;
using PixelGraph.Tests.Internal.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests
{
    public class ConversionTests : TestBase
    {
        public ConversionTests(ITestOutputHelper output) : base(output) {}

        [Fact]
        public void ReplaceLegacyJavaToBedrockMappings()
        {
            using var provider = Builder.Build();
            var javaToBedrock = provider.GetRequiredService<IJavaToBedrockPublishMapping>();

            var total = javaToBedrock.Mappings.Count;

            var mappedCount = 0;
            var unmappedList = new List<string>();
            var invalidList = new List<string>();

            foreach (var (javaFilename, bedrockFilename) in javaToBedrock.Mappings) {
                var javaBlockId = javaFilename.TrimStart("assets/minecraft/textures/block/");
                var expectedBedrockBlockId = bedrockFilename.TrimStart("textures/blocks/");
                
                var javaTextureList = Minecraft.Java
                    .FindBlockTextureVersionById<JavaBlockTextureVersion>(javaBlockId)
                    .Where(tex => tex.MapsToBedrockBlock != null);

                var isMapped = false;
                foreach (var javaTexture in javaTextureList) {
                    var bedrockTexture = (BedrockBlockTexture)Activator.CreateInstance(javaTexture.MapsToBedrockBlock);
                    if (bedrockTexture == null) continue;

                    mappedCount++;
                    isMapped = true;

                    var isValid = bedrockTexture.Versions.Any(v => string.Equals(v.Id, expectedBedrockBlockId, StringComparison.InvariantCultureIgnoreCase));
                    if (!isValid) invalidList.Add($"{javaTexture.Id}: expected '{expectedBedrockBlockId}'");

                    break;
                }

                if (!isMapped) unmappedList.Add(javaFilename);
            }

            Output.WriteLine($"Successfully mapped {mappedCount:N0} of {total:N0} legacy JavaToBedrock mappings.");
            Output.WriteLine($"Found {invalidList.Count:N0} non-matching mappings.");

            if (invalidList.Count > 0)
                Output.WriteLine($"\nInvalid mappings:\n{string.Join('\n', invalidList.Select(x => $"- {x}"))}");

            if (unmappedList.Count > 0)
                Output.WriteLine($"\nFailed to map:\n{string.Join('\n', unmappedList.Select(x => $"- {x}"))}");
        }

        [Fact]
        public void AllJavaBlocksConvertToBedrock()
        {
            using var provider = Builder.Build();
            var javaToBedrock = provider.GetRequiredService<IJavaToBedrockPublishMapping>();

            var javaMatchList = javaToBedrock.Mappings.Keys;
            var javaTextureList = Minecraft.Java.AllBlockTextures;

            // TODO: unfinished
        }

        [Fact]
        public void AllBedrockBlocksConvertToJava()
        {
            using var provider = Builder.Build();
            var javaToBedrock = provider.GetRequiredService<IJavaToBedrockPublishMapping>();

            var javaMatchList = javaToBedrock.Mappings.Keys;
            var javaTextureList = Minecraft.Java.AllBlockTextures;

            // TODO: unfinished
        }
    }
}
