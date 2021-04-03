using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.Tests.Internal;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.EncodingChannelTests
{
    public class AlphaTests : ImageTestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public AlphaTests(ITestOutputHelper output) : base(output)
        {
            packInput = new ResourcePackInputProperties {
                Alpha = {
                    Texture = TextureTags.Alpha,
                    Color = ColorChannel.Red,
                    MaxValue = 255m,
                },
            };

            packProfile = new ResourcePackProfileProperties {
                Encoding = {
                    Alpha = {
                        Texture = TextureTags.Albedo,
                        Color = ColorChannel.Alpha,
                        MaxValue = 255m,
                        Power = 1m,
                    },
                },
            };
        }

        [InlineData(  0)]
        [InlineData(100)]
        [InlineData(155)]
        [InlineData(255)]
        [Theory] public async Task PassthroughValue(byte value)
        {
            await using var graph = Graph();

            graph.PackInput = packInput;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
                Alpha = new MaterialAlphaProperties {
                    Value = value,
                },
            };

            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");
            PixelAssert.AlphaEquals(value, image);
        }

        [InlineData(  0)]
        [InlineData(100)]
        [InlineData(155)]
        [InlineData(255)]
        [Theory] public async Task PassthroughTexture(byte value)
        {
            await using var graph = Graph();

            graph.PackInput = packInput;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/alpha.png", value, 0, 0);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");
            PixelAssert.AlphaEquals(value, image);
        }

        //[InlineData(  0, 0.00,   0)]
        //[InlineData(100, 1.00, 100)]
        //[InlineData(100, 0.50,  50)]
        //[InlineData(100, 2.00, 200)]
        //[InlineData(100, 3.00, 255)]
        //[InlineData(200, 0.01,   2)]
        //[Theory] public async Task ScaleValue(decimal value, decimal scale, byte expected)
        //{
        //    await using var graph = Graph();

        //    graph.PackInput = packInput;
        //    graph.PackProfile = packProfile;
        //    graph.Material = new MaterialProperties {
        //        Name = "test",
        //        LocalPath = "assets",
        //        Alpha = new MaterialAlphaProperties {
        //            Value = value,
        //            Scale = scale,
        //        },
        //    };

        //    await graph.ProcessAsync();

        //    using var image = await graph.GetImageAsync("assets/test.png");
        //    PixelAssert.AlphaEquals(expected, image);
        //}

        [InlineData(  0, 0.0,   0)]
        [InlineData(100, 1.0, 100)]
        [InlineData(100, 0.5,  50)]
        [InlineData(100, 2.0, 200)]
        [InlineData(100, 3.0, 255)]
        [InlineData(200, 0.01,  2)]
        [Theory] public async Task ScaleTexture(byte value, decimal scale, byte expected)
        {
            await using var graph = Graph();

            graph.PackInput = packInput;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
                Alpha = new MaterialAlphaProperties {
                    Scale = scale,
                },
            };

            await graph.CreateImageAsync("assets/test/alpha.png", value, 0, 0);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");
            PixelAssert.AlphaEquals(expected, image);
        }
    }
}
