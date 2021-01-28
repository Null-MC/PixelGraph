using System.Threading.Tasks;
using PixelGraph.Common;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.Tests.Internal;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.EncodingTests
{
    public class PorosityTests : ImageTestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public PorosityTests(ITestOutputHelper output) : base(output)
        {
            packInput = new ResourcePackInputProperties {
                Porosity = {
                    Texture = TextureTags.Porosity,
                    Color = ColorChannel.Red,
                },
            };

            packProfile = new ResourcePackProfileProperties {
                Encoding = {
                    Porosity = {
                        Texture = TextureTags.Porosity,
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
            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                },
            };

            await using var graph = Graph(context);
            await graph.CreateImageAsync("assets/test/porosity.png", value, 0, 0);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_p.png");
            PixelAssert.RedEquals(value, image);
        }

        [InlineData(       0, 0.00,   0)]
        [InlineData(       1, 0.00,   0)]
        [InlineData(100/255d, 1.00, 100)]
        [InlineData(100/255d, 0.50,  50)]
        [InlineData(100/255d, 2.00, 200)]
        [InlineData(100/255d, 3.00, 255)]
        [InlineData(200/255d, 0.01,   2)]
        [Theory] public async Task ScaleValue(decimal value, decimal scale, byte expected)
        {
            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                    Porosity = new MaterialPorosityProperties {
                        Value = value,
                        Scale = scale,
                    },
                },
            };

            await using var graph = Graph(context);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_p.png");
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
            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                    Porosity = new MaterialPorosityProperties {
                        Scale = scale,
                    },
                },
            };

            await using var graph = Graph(context);
            await graph.CreateImageAsync("assets/test/porosity.png", value, 0, 0);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_p.png");
            PixelAssert.RedEquals(expected, image);
        }
    }
}
