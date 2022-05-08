using PixelGraph.Common;
using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.Tests.Internal;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.EncodingChannelTests
{
    public class RoughTests : ImageTestBase
    {
        private readonly ProjectData project;
        private readonly PublishProfileProperties packProfile;


        public RoughTests(ITestOutputHelper output) : base(output)
        {
            Builder.ConfigureReader(ContentTypes.File, GameEditions.None, null);
            Builder.ConfigureWriter(ContentTypes.File, GameEditions.None, null);

            project = new ProjectData {
                Input = new PackInputEncoding {
                    Rough = {
                        Texture = TextureTags.Rough,
                        Color = ColorChannel.Red,
                    },
                },
            };

            packProfile = new PublishProfileProperties {
                Encoding = {
                    Rough = {
                        Texture = TextureTags.Rough,
                        Color = ColorChannel.Red,
                    },
                },
            };
        }

        [InlineData(  0)]
        [InlineData(100)]
        [InlineData(155)]
        [InlineData(255)]
        [Theory] public async Task Passthrough(byte value)
        {
            await using var graph = Graph();

            graph.Project = project;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/rough.png", value);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_rough.png");
            PixelAssert.RedEquals(value, image);
        }

        //[InlineData(0.000, 0.00,   0)]
        //[InlineData(1.000, 0.00,   0)]
        //[InlineData(0.392, 1.00, 100)]
        //[InlineData(0.392, 0.50,  50)]
        //[InlineData(0.392, 2.00, 200)]
        //[InlineData(0.392, 3.00, 255)]
        //[InlineData(0.784, 0.01,   2)]
        //[Theory] public async Task ScaleValue(decimal value, decimal scale, byte expected)
        //{
        //    await using var graph = Graph();

        //    graph.PackInput = packInput;
        //    graph.PackProfile = packProfile;
        //    graph.Material = new MaterialProperties {
        //        Name = "test",
        //        LocalPath = "assets",
        //        Rough = new MaterialRoughProperties {
        //            Value = value,
        //            Scale = scale,
        //        },
        //    };

        //    await graph.ProcessAsync();

        //    using var image = await graph.GetImageAsync("assets/test_rough.png");
        //    PixelAssert.RedEquals(expected, image);
        //}

        [InlineData(  0,  0.0,   0)]
        [InlineData(100,  1.0, 100)]
        [InlineData(100,  0.5,  50)]
        [InlineData(100,  2.0, 200)]
        [InlineData(100,  3.0, 255)]
        [InlineData(200, 0.01,   2)]
        [Theory] public async Task ScaleTexture(byte value, decimal scale, byte expected)
        {
            await using var graph = Graph();

            graph.Project = project;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
                Rough = new MaterialRoughProperties {
                    Scale = scale,
                },
            };

            await graph.CreateImageAsync("assets/test/rough.png", value);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_rough.png");
            PixelAssert.RedEquals(expected, image);
        }

        [InlineData(  0, 255)]
        [InlineData(100, 155)]
        [InlineData(155, 100)]
        [InlineData(255,   0)]
        [Theory] public async Task ConvertsSmoothToRough(byte value, byte expected)
        {
            await using var graph = Graph();

            graph.Project = new ProjectData {
                Input = new PackInputEncoding {
                    Smooth = {
                        Texture = TextureTags.Smooth,
                        Color = ColorChannel.Red,
                    },
                },
            };

            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/smooth.png", value);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_rough.png");
            PixelAssert.RedEquals(expected, image);
        }
    }
}
