using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO.Importing;
using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Tests.Internal;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.ImportTests
{
    public class BedrockImportTests : ImageTestBase
    {
        private readonly ProjectData project;
        private readonly PublishProfileProperties packProfile;


        public BedrockImportTests(ITestOutputHelper output) : base(output)
        {
            Builder.ConfigureReader(ContentTypes.File, GameEditions.Bedrock, null);
            Builder.ConfigureWriter(ContentTypes.File, GameEditions.None, null);
            Builder.AddImporter(GameEditions.Bedrock);

            project = new ProjectData {
                Input = new PackInputEncoding {
                    //Edition = GameEditions.Java,
                    Format = TextureFormat.Format_Raw,
                },
            };

            packProfile = new PublishProfileProperties {
                //Edition = GameEditions.Bedrock,
                Encoding = {
                    Format = TextureFormat.Format_Color,
                },
            };
        }

        [Fact]
        public async Task CanImportLocal()
        {
            await using var graph = Graph();

            await graph.CreateImageAsync("textures/blocks/brick.png", 31, 156, 248);

            var importer = graph.Provider.GetRequiredService<IMaterialImporter>();

            importer.Project = project;
            importer.PackProfile = packProfile;
            importer.AsGlobal = false;

            //var localPath = PathEx.Localize("textures/blocks");
            
            //var srcMaterial = await importer.CreateMaterialAsync(localPath, "brick");
            //var matFile = AsGlobal
            //    ? PathEx.Join(localPath, $"{name}.mat.yml")
            //    : PathEx.Join(localPath, name, "mat.yml");

            var material = new MaterialProperties {
                Name = "brick",
                LocalPath = PathEx.Localize("textures/blocks"),
                //LocalFilename = matFile,
                UseGlobalMatching = true,
            };

            //await MaterialWriter.WriteAsync(material);


            await importer.ImportAsync(material);

            //await using var material = graph.GetFile("assets/minecraft/textures/block/bricks/mat.yml");
            //Assert.NotNull(material);

            using var colorImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/color.png");
            PixelAssert.Equals(31, 156, 248, colorImage);

            using var opacityImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/opacity.png");
            PixelAssert.Equals(255, opacityImage);
        }
    }
}
