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
    public class PerceptualSmoothTests : ImageTestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public PerceptualSmoothTests(ITestOutputHelper output) : base(output)
        {
            packInput = new ResourcePackInputProperties {
                Smooth = {
                    Texture = TextureTags.Smooth,
                    Color = ColorChannel.Red,
                    Perceptual = true,
                }
            };

            packProfile = new ResourcePackProfileProperties {
                Encoding = {
                    Smooth = {
                        Texture = TextureTags.Smooth,
                        Color = ColorChannel.Red,
                        Perceptual = true,
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
            await graph.CreateImageAsync("assets/test/smooth.png", value, 0, 0);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_smooth.png");
            PixelAssert.RedEquals(value, image);
        }

        [InlineData(  0,   0)]
        [InlineData(100,  56)]
        [InlineData(200, 137)]
        [InlineData(255, 255)]
        [Theory] public async Task ConvertsSmoothToPerceptualSmooth(byte value, byte expected)
        {
            var context = new MaterialContext {
                Input = new ResourcePackInputProperties {
                    Smooth = {
                        Texture = TextureTags.Smooth,
                        Color = ColorChannel.Red,
                    },
                },
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                },
            };

            await using var graph = Graph(context);
            await graph.CreateImageAsync("assets/test/smooth.png", value, 0, 0);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_smooth.png");
            PixelAssert.RedEquals(expected, image);
        }

        [InlineData(255,   0)]
        [InlineData(200,  29)]
        [InlineData(100,  95)]
        [InlineData( 50, 142)]
        [InlineData(  0, 255)]
        [Theory] public async Task ConvertsRoughToPerceptualSmooth(byte value, byte expected)
        {
            var context = new MaterialContext {
                Input = new ResourcePackInputProperties {
                    Rough = {
                        Texture = TextureTags.Rough,
                        Color = ColorChannel.Red,
                        Perceptual = false,
                    },
                },
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                },
            };

            await using var graph = Graph(context);
            await graph.CreateImageAsync("assets/test/rough.png", value, 0, 0);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_smooth.png");
            PixelAssert.RedEquals(expected, image);
        }
    }
}
