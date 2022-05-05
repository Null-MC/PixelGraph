using PixelGraph.Common;
using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Tests.Internal;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.PublishTests
{
    public class JavaOldPbrPublishTests : ImageTestBase
    {
        private readonly ProjectData project;
        private readonly PublishProfileProperties packProfile;


        public JavaOldPbrPublishTests(ITestOutputHelper output) : base(output)
        {
            Builder.ConfigureReader(ContentTypes.File, GameEditions.None, null);
            Builder.ConfigureWriter(ContentTypes.File, GameEditions.Java, null);

            project = new ProjectData {
                Input = new PackInputEncoding {
                    Format = TextureFormat.Format_Raw,
                },
            };

            packProfile = new PublishProfileProperties {
                Encoding = {
                    Format = TextureFormat.Format_OldPbr,
                },
            };
        }

        [InlineData(  0)]
        [InlineData(100)]
        [InlineData(155)]
        [InlineData(255)]
        [Theory] public async Task OpacityTextureTest(byte value)
        {
            await using var graph = Graph();

            graph.Project = project;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/opacity.png", value);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");
            PixelAssert.AlphaEquals(value, image);
        }

        [InlineData(  0,   0,  0)]
        [InlineData(100, 101, 102)]
        [InlineData(155, 160, 165)]
        [InlineData(255, 255, 255)]
        [Theory] public async Task ColorTextureTest(byte red, byte green, byte blue)
        {
            await using var graph = Graph();

            graph.Project = project;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/color.png", red, green, blue);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");
            PixelAssert.RedEquals(red, image);
            PixelAssert.GreenEquals(green, image);
            PixelAssert.BlueEquals(blue, image);
        }

        [InlineData(127, 127, 255)]
        [InlineData(127,   0, 127)]
        [InlineData(127, 255, 127)]
        [InlineData(  0, 127, 127)]
        [InlineData(255, 127, 127)]
        [Theory] public async Task NormalTextureTest(byte red, byte green, byte blue)
        {
            await using var graph = Graph();

            graph.Project = project;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/normal.png", red, green, blue);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_n.png");
            PixelAssert.RedEquals(red, image);
            PixelAssert.GreenEquals(green, image);
            PixelAssert.BlueEquals(blue, image);
        }

        [InlineData(  0)]
        [InlineData(100)]
        [InlineData(155)]
        [InlineData(255)]
        [Theory] public async Task HeightTextureTest(byte value)
        {
            await using var graph = Graph();

            graph.Project = project;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/height.png", value);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_n.png");
            PixelAssert.AlphaEquals(value, image);
        }

        [InlineData(  0)]
        [InlineData(100)]
        [InlineData(155)]
        [InlineData(255)]
        [Theory] public async Task SmoothTextureTest(byte value)
        {
            await using var graph = Graph();

            graph.Project = project;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/smooth.png", value);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_s.png");
            PixelAssert.RedEquals(value, image);
        }

        [InlineData(0)]
        [InlineData(255)]
        [Theory] public async Task MetalTextureTest(byte value)
        {
            await using var graph = Graph();

            graph.Project = project;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/metal.png", value);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_s.png");
            PixelAssert.GreenEquals(value, image);
        }

        [InlineData(  0)]
        [InlineData(  1)]
        [InlineData(100)]
        [InlineData(155)]
        [InlineData(255)]
        [Theory] public async Task EmissiveTextureTest(byte value)
        {
            await using var graph = Graph();

            graph.Project = project;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/emissive.png", value);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_s.png");
            PixelAssert.BlueEquals(value, image);
        }
    }
}
