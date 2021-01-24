using PixelGraph.Common;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.Tests.Internal;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.ImageTests
{
    public class OcclusionTests : ImageTestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public OcclusionTests(ITestOutputHelper output) : base(output)
        {
            packInput = new ResourcePackInputProperties {
                Occlusion = {
                    Texture = TextureTags.Occlusion,
                    Color = ColorChannel.Red,
                    Invert = true,
                },
            };

            packProfile = new ResourcePackProfileProperties {
                Encoding = {
                    Occlusion = {
                        Texture = TextureTags.Occlusion,
                        Color = ColorChannel.Red,
                        Invert = true,
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
            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                },
            };

            await using var graph = Graph(context);
            await graph.CreateImageAsync("assets/test/occlusion.png", value, 0, 0);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_ao.png");
            PixelAssert.RedEquals(value, image);
        }

        [InlineData(0.000,  0.0f, 255)]
        [InlineData(1.000,  0.0f, 255)]
        [InlineData(0.392,  1.0f, 155)]
        [InlineData(0.392,  0.5f, 205)]
        [InlineData(0.392,  2.0f,  55)]
        [InlineData(0.392,  3.0f,   0)]
        [InlineData(0.784, 0.01f, 253)]
        [Theory] public async Task ScaleValue(decimal value, decimal scale, byte expected)
        {
            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                    Occlusion = new MaterialOcclusionProperties {
                        Value = value,
                        Scale = scale,
                    },
                },
            };

            await using var graph = Graph(context);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_ao.png");
            PixelAssert.RedEquals(expected, image);
        }

        [InlineData(  0,  0.0, 255)]
        [InlineData(255,  0.0, 255)]
        [InlineData(100,  1.0, 100)]
        [InlineData(155,  0.5, 205)]
        [InlineData(155,  2.0,  55)]
        [InlineData(155,  3.0,   0)]
        [InlineData( 55, 0.01, 253)]
        [Theory] public async Task ScaleTexture(byte value, decimal scale, byte expected)
        {
            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                    Occlusion = new MaterialOcclusionProperties {
                        Scale = scale,
                    },
                },
            };

            await using var graph = Graph(context);
            await graph.CreateImageAsync("assets/test/occlusion.png", value, 0, 0);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_ao.png");
            PixelAssert.RedEquals(expected, image);
        }
    }
}
