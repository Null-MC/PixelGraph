using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Encoding;
using PixelGraph.Common.IO.Importing;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Tests.Internal;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.ImportTests
{
    public class RawImportTests : ImageTestBase
    {
        public RawImportTests(ITestOutputHelper output) : base(output) {}

        [Fact]
        public async Task CanImportAlbedoLocal()
        {
            await using var graph = Graph();

            var importer = graph.Provider.GetRequiredService<IMaterialImporter>();
            importer.LocalPath = "assets/minecraft/textures/block";
            importer.AsGlobal = false;

            importer.PackInput = new ResourcePackInputProperties {
                Format = TextureEncoding.Format_Raw,
                //...
            };

            importer.PackProfile = new ResourcePackProfileProperties {
                Edition = "Java",
                Encoding = {
                    Format = TextureEncoding.Format_Raw,
                },
                //...
            };

            await graph.CreateImageAsync("assets/minecraft/textures/block/bricks_albedo.png", 255, 0, 0);

            await importer.ImportAsync("bricks");

            await using var material = graph.GetFile("assets/minecraft/textures/block/bricks/mat.yml");
            Assert.NotNull(material);

            // TODO: validate material?

            using var image = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/albedo.png");
            PixelAssert.Equals(255, 0, 0, image);
        }
    }
}
