using System.Threading.Tasks;
using PixelGraph.Common;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.Tests.Internal;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.EncodingChannelTests
{
    public class SubSurfaceScatteringTests : ImageTestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public SubSurfaceScatteringTests(ITestOutputHelper output) : base(output)
        {
            packInput = new ResourcePackInputProperties {
                SSS = {
                    Texture = TextureTags.SubSurfaceScattering,
                    Color = ColorChannel.Red,
                },
            };

            packProfile = new ResourcePackProfileProperties {
                Encoding = {
                    SSS = {
                        Texture = TextureTags.SubSurfaceScattering,
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
            using var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                },
            };

            await using var graph = Graph(context);
            await graph.CreateImageAsync("assets/test/sss.png", value, 0, 0);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_sss.png");
            PixelAssert.RedEquals(value, image);
        }

        [InlineData(0.000, 0.00,   0)]
        [InlineData(1.000, 0.00,   0)]
        [InlineData(0.392, 1.00, 100)]
        [InlineData(0.392, 0.50,  50)]
        [InlineData(0.392, 2.00, 200)]
        [InlineData(0.392, 3.00, 255)]
        [InlineData(0.784, 0.01,   2)]
        [Theory] public async Task ScaleValue(decimal value, decimal scale, byte expected)
        {
            using var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                    SSS = new MaterialSssProperties {
                        Value = value,
                        Scale = scale,
                    },
                },
            };

            await using var graph = Graph(context);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_sss.png");
            PixelAssert.RedEquals(expected, image);
        }

        [InlineData(  0, 0.00,   0)]
        [InlineData(100, 1.00, 100)]
        [InlineData(100, 0.50,  50)]
        [InlineData(100, 2.00, 200)]
        [InlineData(100, 3.00, 255)]
        [InlineData(200, 0.01,   2)]
        [Theory] public async Task ScaleTexture(byte value, decimal scale, byte expected)
        {
            using var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                    SSS = new MaterialSssProperties {
                        Scale = scale,
                    },
                },
            };

            await using var graph = Graph(context);
            await graph.CreateImageAsync("assets/test/sss.png", value, 0, 0);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_sss.png");
            PixelAssert.RedEquals(expected, image);
        }
    }
}
