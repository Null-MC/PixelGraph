using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO.Importing;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Tests.Internal;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.ImportTests
{
    public class OldPbrImportTests : ImageTestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public OldPbrImportTests(ITestOutputHelper output) : base(output)
        {
            packInput = new ResourcePackInputProperties {
                Format = TextureFormat.Format_Raw,
            };

            packProfile = new ResourcePackProfileProperties {
                Encoding = {
                    Format = TextureFormat.Format_OldPbr,
                },
            };
        }

        [Fact]
        public async Task CanImportLocal()
        {
            await using var graph = Graph();

            await graph.CreateImageAsync("assets/minecraft/textures/block/bricks.png", 31, 156, 248);
            await graph.CreateImageAsync("assets/minecraft/textures/block/bricks_n.png", 127, 127, 255, 250);
            await graph.CreateImageAsync("assets/minecraft/textures/block/bricks_s.png", 16, 0, 0);

            var importer = graph.Provider.GetRequiredService<IMaterialImporter>();
            importer.LocalPath = PathEx.Localize("assets/minecraft/textures/block");
            importer.PackInput = packInput;
            importer.PackProfile = packProfile;
            importer.AsGlobal = false;

            await importer.ImportAsync("bricks");

            await using var material = graph.GetFile("assets/minecraft/textures/block/bricks/mat.yml");
            Assert.NotNull(material);

            using var albedoImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/albedo.png");
            PixelAssert.Equals(31, 156, 248, albedoImage);

            using var normalImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/normal.png");
            PixelAssert.Equals(127, 127, 255, normalImage);

            using var heightImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/height.png");
            PixelAssert.Equals(250, heightImage);

            using var smoothImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/smooth.png");
            PixelAssert.Equals(16, smoothImage);

            using var metalImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/metal.png");
            PixelAssert.Equals(0, metalImage);

            using var emissiveImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/emissive.png");
            PixelAssert.Equals(0, emissiveImage);
        }
    }
}
