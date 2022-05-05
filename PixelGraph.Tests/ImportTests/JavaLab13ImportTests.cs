using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Importing;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Tests.Internal;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.ImportTests
{
    public class JavaLab13ImportTests : ImageTestBase
    {
        private readonly ProjectData project;
        private readonly PublishProfileProperties packProfile;


        public JavaLab13ImportTests(ITestOutputHelper output) : base(output)
        {
            Builder.ConfigureReader(ContentTypes.File, GameEditions.Java, null);
            Builder.ConfigureWriter(ContentTypes.File, GameEditions.None, null);
            Builder.AddImporter(GameEditions.Java);

            project = new ProjectData {
                Input = new PackInputEncoding {
                    Edition = GameEdition.Java,
                    Format = TextureFormat.Format_Raw,
                },
            };

            packProfile = new PublishProfileProperties {
                Edition = GameEdition.Java,
                Encoding = {
                    Format = TextureFormat.Format_Lab13,
                },
            };
        }

        [Fact]
        public async Task CanImportLocal()
        {
            await using var graph = Graph();

            await graph.CreateImageAsync("assets/minecraft/textures/block/bricks.png", 31, 156, 248);
            await graph.CreateImageAsync("assets/minecraft/textures/block/bricks_n.png", 127, 127, 200, 250);
            await graph.CreateImageAsync("assets/minecraft/textures/block/bricks_s.png", 16, 8, 45, 255);

            var importer = graph.Provider.GetRequiredService<IMaterialImporter>();
            importer.Project = project;
            importer.PackProfile = packProfile;
            importer.AsGlobal = false;

            var localPath = PathEx.Localize("assets/minecraft/textures/block");
            var srcMaterial = await importer.CreateMaterialAsync(localPath, "bricks");
            await importer.ImportAsync(srcMaterial);

            await using var material = graph.GetFile("assets/minecraft/textures/block/bricks/mat.yml");
            Assert.NotNull(material);

            using var albedoImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/color.png");
            PixelAssert.Equals(31, 156, 248, albedoImage);

            using var normalImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/normal.png");
            PixelAssert.Equals(127, 127, 255, normalImage);

            using var heightImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/height.png");
            PixelAssert.Equals(250, heightImage);

            using var occlusionImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/occlusion.png");
            PixelAssert.Equals(200, occlusionImage);

            using var smoothImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/smooth.png");
            PixelAssert.Equals(16, smoothImage);

            using var f0Image = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/f0.png");
            PixelAssert.Equals(8, f0Image);

            using var hcmImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/hcm.png");
            PixelAssert.Equals(0, hcmImage);

            using var porosityImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/porosity.png");
            PixelAssert.Equals(179, porosityImage);

            using var sssImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/sss.png");
            PixelAssert.Equals(0, sssImage);

            using var emissiveImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/emissive.png");
            PixelAssert.Equals(0, emissiveImage);
        }
    }
}
