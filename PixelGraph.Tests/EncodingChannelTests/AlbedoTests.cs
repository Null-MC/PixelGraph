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
    public class AlbedoTests : ImageTestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public AlbedoTests(ITestOutputHelper output) : base(output)
        {
            packInput = new ResourcePackInputProperties {
                AlbedoRed = {
                    Texture = TextureTags.Albedo,
                    Color = ColorChannel.Red,
                    MaxValue = 255m,
                },
                AlbedoGreen = {
                    Texture = TextureTags.Albedo,
                    Color = ColorChannel.Green,
                    MaxValue = 255m,
                },
                AlbedoBlue = {
                    Texture = TextureTags.Albedo,
                    Color = ColorChannel.Blue,
                    MaxValue = 255m,
                },
            };

            packProfile = new ResourcePackProfileProperties {
                Encoding = {
                    AlbedoRed = {
                        Texture = TextureTags.Albedo,
                        Color = ColorChannel.Red,
                        MaxValue = 255m,
                    },
                    AlbedoGreen = {
                        Texture = TextureTags.Albedo,
                        Color = ColorChannel.Green,
                        MaxValue = 255m,
                    },
                    AlbedoBlue = {
                        Texture = TextureTags.Albedo,
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
            using var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                },
            };

            await using var graph = Graph(context);
            await graph.CreateImageAsync("assets/test/albedo.png", value, 0, 0);
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
            using var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                },
            };

            await using var graph = Graph(context);
            await graph.CreateImageAsync("assets/test/albedo.png", 0, value, 0);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");
            PixelAssert.GreenEquals(value, image);
        }

        [InlineData(  0, 0.00,   0)]
        [InlineData(100, 1.00, 100)]
        [InlineData(100, 0.50,  50)]
        [InlineData(100, 2.00, 200)]
        [InlineData(100, 3.00, 255)]
        [InlineData(200, 0.01,   2)]
        [Theory] public async Task ScaleRedValue(decimal value, decimal scale, byte expected)
        {
            using var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                    Albedo = new MaterialAlbedoProperties {
                        ValueRed = value,
                        ScaleRed = scale,
                    },
                },
            };

            await using var graph = Graph(context);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");
            PixelAssert.RedEquals(expected, image);
        }

        [InlineData(  0, 0.0,   0)]
        [InlineData(100, 1.0, 100)]
        [InlineData(100, 0.5,  50)]
        [InlineData(100, 2.0, 200)]
        [InlineData(100, 3.0, 255)]
        [InlineData(200, 0.01,  2)]
        [Theory] public async Task ScaleRedTexture(byte value, decimal scale, byte expected)
        {
            using var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                    Albedo = new MaterialAlbedoProperties {
                        ScaleRed = scale,
                    },
                },
            };

            await using var graph = Graph(context);
            await graph.CreateImageAsync("assets/test/albedo.png", value, 0, 0);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");
            PixelAssert.RedEquals(expected, image);
        }

        [Fact]
        public async Task ShiftRgb()
        {
            using var context = new MaterialContext {
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                },
                Input = new ResourcePackInputProperties {
                    AlbedoRed = {
                        Texture = TextureTags.Albedo,
                        Color = ColorChannel.Green,
                        MaxValue = 255m,
                    },
                    AlbedoGreen = {
                        Texture = TextureTags.Albedo,
                        Color = ColorChannel.Blue,
                        MaxValue = 255m,
                    },
                    AlbedoBlue = {
                        Texture = TextureTags.Albedo,
                        Color = ColorChannel.Red,
                        MaxValue = 255m,
                    },
                },
            };

            await using var graph = Graph(context);
            await graph.CreateImageAsync("assets/test/albedo.png", 60, 120, 180);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");
            PixelAssert.RedEquals(120, image);
            PixelAssert.GreenEquals(180, image);
            PixelAssert.BlueEquals(60, image);
        }
    }
}
