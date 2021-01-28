using PixelGraph.Common;
using PixelGraph.Common.Encoding;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Tests.Internal;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.EncodingFormatTests
{
    public class Lab13Tests : ImageTestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public Lab13Tests(ITestOutputHelper output) : base(output)
        {
            packInput = new ResourcePackInputProperties {
                Format = TextureEncoding.Format_Raw,
            };

            packProfile = new ResourcePackProfileProperties {
                Encoding = {
                    Format = TextureEncoding.Format_Lab13,
                },
            };
        }

        [InlineData(  0)]
        [InlineData(100)]
        [InlineData(155)]
        [InlineData(255)]
        [Theory] public async Task AlphaTextureTest(byte value)
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
            await graph.CreateImageAsync("assets/test/alpha.png", value);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");
            PixelAssert.AlphaEquals(value, image);
        }

        [InlineData(  0,   0,  0)]
        [InlineData(100, 101, 102)]
        [InlineData(155, 160, 165)]
        [InlineData(255, 255, 255)]
        [Theory] public async Task AlbedoTextureTest(byte red, byte green, byte blue)
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
            await graph.CreateImageAsync("assets/test/albedo.png", red, green, blue);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");
            PixelAssert.RedEquals(red, image);
            PixelAssert.GreenEquals(green, image);
            PixelAssert.BlueEquals(blue, image);
        }

        [InlineData(127, 127, 255)]
        [InlineData(127,   0,   0)]
        [InlineData(127, 255,   0)]
        [InlineData(  0, 127,   0)]
        [InlineData(255, 127,   0)]
        [Theory] public async Task NormalTextureTest(byte red, byte green, byte blue)
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
            await graph.CreateImageAsync("assets/test/normal.png", red, green, blue);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_n.png");
            PixelAssert.RedEquals(red, image);
            PixelAssert.GreenEquals(green, image);
            PixelAssert.BlueEquals(255, image);
        }

        [InlineData(  0)]
        [InlineData(100)]
        [InlineData(155)]
        [InlineData(255)]
        [Theory] public async Task OcclusionTextureTest(byte value)
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
            await graph.CreateImageAsync("assets/test/occlusion.png", value);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_n.png");
            PixelAssert.BlueEquals(value, image);
        }

        [InlineData(  0)]
        [InlineData(100)]
        [InlineData(155)]
        [InlineData(255)]
        [Theory] public async Task HeightTextureTest(byte value)
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
            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                },
            };

            await using var graph = Graph(context);
            await graph.CreateImageAsync("assets/test/smooth.png", value);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_s.png");
            PixelAssert.RedEquals(value, image);
        }

        [InlineData(  0)]
        [InlineData(100)]
        [InlineData(155)]
        [InlineData(229)]
        [Theory] public async Task F0TextureTest(byte value)
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
            await graph.CreateImageAsync("assets/test/f0.png", value);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_s.png");
            PixelAssert.GreenEquals(value, image);
        }

        [InlineData(230)]
        [InlineData(231)]
        [InlineData(240)]
        [InlineData(255)]
        [Theory] public async Task MetalTextureTest(byte value)
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
            await graph.CreateImageAsync("assets/test/metal.png", value);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_s.png");
            PixelAssert.GreenEquals(value, image);
        }

        [InlineData(  0,  0)]
        [InlineData(100, 25)]
        [InlineData(155, 39)]
        [InlineData(255, 64)]
        [Theory] public async Task PorosityTextureTest(byte actualValue, byte expectedValue)
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
            await graph.CreateImageAsync("assets/test/porosity.png", actualValue);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_s.png");
            PixelAssert.BlueEquals(expectedValue, image);
        }

        [InlineData(  0,  65)]
        [InlineData(100, 140)]
        [InlineData(155, 180)]
        [InlineData(255, 255)]
        [Theory] public async Task SubSurfaceScatteringTextureTest(byte actualValue, byte expectedValue)
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
            await graph.CreateImageAsync("assets/test/sss.png", actualValue);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_s.png");
            PixelAssert.BlueEquals(expectedValue, image);
        }

        [InlineData(  0, 255)]
        [InlineData(  1,   0)]
        [InlineData(100,  99)]
        [InlineData(155, 154)]
        [InlineData(255, 254)]
        [Theory] public async Task EmissiveTextureTest(byte actualValue, byte expectedValue)
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
            await graph.CreateImageAsync("assets/test/emissive.png", actualValue);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_s.png");
            PixelAssert.AlphaEquals(expectedValue, image);
        }
    }
}
