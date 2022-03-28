using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO.Importing;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Tests.Internal;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.ImportTests
{
    public class BedrockRtxImportTests : ImageTestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public BedrockRtxImportTests(ITestOutputHelper output) : base(output)
        {
            Builder.AddImporter(GameEditions.Bedrock);
            Builder.AddTextureReader(GameEditions.Bedrock);
            Builder.AddTextureWriter(GameEditions.None);

            packInput = new ResourcePackInputProperties {
                //Edition = GameEditions.Java,
                Format = TextureFormat.Format_Raw,
            };

            packProfile = new ResourcePackProfileProperties {
                //Edition = GameEditions.Bedrock,
                Encoding = {
                    Format = TextureFormat.Format_Rtx,
                },
            };
        }

        [Fact]
        public async Task CanImportLocal()
        {
            await using var graph = Graph();

            await graph.CreateImageAsync("textures/blocks/brick.png", 31, 156, 248);
            //await graph.CreateImageAsync("textures/blocks/brick_heightmap.png", 252);
            await graph.CreateImageAsync("textures/blocks/brick_normal.png", 127, 127, 200);
            await graph.CreateImageAsync("textures/blocks/brick_mer.png", 16, 8, 45);

            var importer = graph.Provider.GetRequiredService<IMaterialImporter>();
            importer.PackInput = packInput;
            importer.PackProfile = packProfile;
            importer.AsGlobal = false;

            var material = new MaterialProperties {
                Name = "brick",
                LocalPath = PathEx.Localize("textures/blocks"),
                //LocalFilename = matFile,
                UseGlobalMatching = true,
            };

            await importer.ImportAsync(material);

            //await using var material = graph.GetFile("assets/minecraft/textures/block/bricks/mat.yml");
            //Assert.NotNull(material);

            using var albedoImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/color.png");
            PixelAssert.Equals(31, 156, 248, albedoImage);

            using var opacityImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/opacity.png");
            PixelAssert.Equals(255, opacityImage);

            using var normalImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/normal.png");
            PixelAssert.Equals(127, 127, 255, normalImage);

            //using var heightImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/height.png");
            //PixelAssert.Equals(252, heightImage);

            using var metalImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/metal.png");
            PixelAssert.Equals(16, metalImage);

            using var emissiveImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/emissive.png");
            PixelAssert.Equals(8, emissiveImage);

            using var roughImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/rough.png");
            PixelAssert.Equals(45, roughImage);
        }

        //[InlineData(  0,   0)]
        //[InlineData(127,   0)]
        //[InlineData(128,   0)]
        //[InlineData(129,   2)]
        //[InlineData(200, 145)]
        //[InlineData(254, 253)]
        //[InlineData(255, 255)]
        //[Theory] public async Task OpacityTest(byte inputValue, byte outputValue)
        //{
        //    await using var graph = Graph();
        //    await graph.CreateImageAsync("textures/blocks/brick.png", 0, 0, 0, inputValue);

        //    var importer = graph.Provider.GetRequiredService<IMaterialImporter>();
        //    importer.PackInput = packInput;
        //    importer.PackProfile = packProfile;
        //    importer.AsGlobal = false;

        //    var material = new MaterialProperties {
        //        Name = "brick",
        //        LocalPath = PathEx.Localize("textures/blocks"),
        //        UseGlobalMatching = true,
        //    };

        //    await importer.ImportAsync(material);
        //    using var opacityImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/opacity.png");
        //    PixelAssert.Equals(outputValue, opacityImage);
        //}
    }
}
