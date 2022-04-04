using PixelGraph.Common;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.Tests.Internal;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.EncodingChannelTests
{
    public class ColorTests : ImageTestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public ColorTests(ITestOutputHelper output) : base(output)
        {
            Builder.ConfigureReader(ContentTypes.File, GameEditions.None, null);
            Builder.ConfigureWriter(ContentTypes.File, GameEditions.None, null);

            packInput = new ResourcePackInputProperties {
                ColorRed = {
                    Texture = TextureTags.Color,
                    Color = ColorChannel.Red,
                    MaxValue = 255m,
                },
                ColorGreen = {
                    Texture = TextureTags.Color,
                    Color = ColorChannel.Green,
                    MaxValue = 255m,
                },
                ColorBlue = {
                    Texture = TextureTags.Color,
                    Color = ColorChannel.Blue,
                    MaxValue = 255m,
                },
            };

            packProfile = new ResourcePackProfileProperties {
                Encoding = {
                    ColorRed = {
                        Texture = TextureTags.Color,
                        Color = ColorChannel.Red,
                        MaxValue = 255m,
                    },
                    ColorGreen = {
                        Texture = TextureTags.Color,
                        Color = ColorChannel.Green,
                        MaxValue = 255m,
                    },
                    ColorBlue = {
                        Texture = TextureTags.Color,
                        Color = ColorChannel.Blue,
                        MaxValue = 255m,
                    },
                },
            };
        }

        [InlineData(  0)]
        [InlineData(100)]
        [InlineData(155)]
        [InlineData(255)]
        [Theory] public async Task RedPassthrough(byte value)
        {
            await using var graph = Graph();

            graph.PackInput = packInput;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/color.png", value, 0, 0);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");
            PixelAssert.RedEquals(value, image);
        }

        [InlineData(  0)]
        [InlineData(100)]
        [InlineData(155)]
        [InlineData(255)]
        [Theory] public async Task GreenPassthrough(byte value)
        {
            await using var graph = Graph();

            graph.PackInput = packInput;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/color.png", 0, value, 0);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");
            PixelAssert.GreenEquals(value, image);
        }

        //[InlineData(  0, 0.00,   0)]
        //[InlineData(100, 1.00, 100)]
        //[InlineData(100, 0.50,  50)]
        //[InlineData(100, 2.00, 200)]
        //[InlineData(100, 3.00, 255)]
        //[InlineData(200, 0.01,   2)]
        //[Theory] public async Task ScaleRedValue(decimal value, decimal scale, byte expected)
        //{
        //    await using var graph = Graph();

        //    graph.PackInput = packInput;
        //    graph.PackProfile = packProfile;
        //    graph.Material = new MaterialProperties {
        //        Name = "test",
        //        LocalPath = "assets",
        //        Albedo = new MaterialAlbedoProperties {
        //            ValueRed = value,
        //            ScaleRed = scale,
        //        },
        //    };

        //    await graph.ProcessAsync();

        //    using var image = await graph.GetImageAsync("assets/test.png");
        //    PixelAssert.RedEquals(expected, image);
        //}

        [InlineData(  0, 0.0,   0)]
        [InlineData(100, 1.0, 100)]
        [InlineData(100, 0.5,  50)]
        [InlineData(100, 2.0, 200)]
        [InlineData(100, 3.0, 255)]
        [InlineData(200, 0.01,  2)]
        [Theory] public async Task ScaleRedTexture(byte value, decimal scale, byte expected)
        {
            await using var graph = Graph();

            graph.PackInput = packInput;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
                Color = new MaterialColorProperties {
                    ScaleRed = scale,
                },
            };

            await graph.CreateImageAsync("assets/test/color.png", value, 0, 0);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");
            PixelAssert.RedEquals(expected, image);
        }

        [Fact]
        public async Task ShiftRgb()
        {
            await using var graph = Graph();

            graph.PackInput = new ResourcePackInputProperties {
                ColorRed = {
                    Texture = TextureTags.Color,
                    Color = ColorChannel.Green,
                    MaxValue = 255m,
                },
                ColorGreen = {
                    Texture = TextureTags.Color,
                    Color = ColorChannel.Blue,
                    MaxValue = 255m,
                },
                ColorBlue = {
                    Texture = TextureTags.Color,
                    Color = ColorChannel.Red,
                    MaxValue = 255m,
                },
            };
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/color.png", 60, 120, 180);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");
            PixelAssert.RedEquals(120, image);
            PixelAssert.GreenEquals(180, image);
            PixelAssert.BlueEquals(60, image);
        }
    }
}
