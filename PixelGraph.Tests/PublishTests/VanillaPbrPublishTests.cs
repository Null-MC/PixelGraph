using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Tests.Internal;
using System.Threading.Tasks;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.PublishTests
{
    public class VanillaPbrPublishTests : ImageTestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public VanillaPbrPublishTests(ITestOutputHelper output) : base(output)
        {
            packInput = new ResourcePackInputProperties {
                Format = TextureFormat.Format_Raw,
            };

            packProfile = new ResourcePackProfileProperties {
                Encoding = {
                    Format = TextureFormat.Format_VanillaPbr,
                },
            };
        }

        [InlineData(  0,   0,  0)]
        [InlineData(100, 101, 102)]
        [InlineData(155, 160, 165)]
        [InlineData(255, 255, 255)]
        [Theory] public async Task ColorTextureTest(byte red, byte green, byte blue)
        {
            await using var graph = Graph();

            graph.PackInput = packInput;
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

        [InlineData(  0,  0)]
        [InlineData(100, 73)]
        [InlineData(155, 73)]
        [InlineData(255, 73)]
        [Theory] public async Task OpacityValueTest(byte actualValue, byte expectedValue)
        {
            await using var graph = Graph();

            graph.PackInput = packInput;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
                Opacity = new MaterialOpacityProperties {
                    Value = actualValue,
                }
            };

            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");
            PixelAssert.AlphaEquals(expectedValue, image);
        }

        [InlineData(  0,  0)]
        [InlineData(100, 73)]
        [InlineData(155, 73)]
        [InlineData(255, 73)]
        [Theory] public async Task OpacityTextureTest(byte actualValue, byte expectedValue)
        {
            await using var graph = Graph();

            graph.PackInput = packInput;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/opacity.png", actualValue);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");
            PixelAssert.AlphaEquals(expectedValue, image);
        }

        [InlineData(0.0, 73)]
        [InlineData(0.1, 25)]
        [InlineData(0.5, 35)]
        [InlineData(0.9, 45)]
        [InlineData(1.0, 47)]
        [Theory] public async Task SubSurfaceScatteringValueTest(decimal actualValue, byte expectedValue)
        {
            await using var graph = Graph();

            graph.PackInput = packInput;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
                SSS = new MaterialSssProperties {
                    Value = actualValue,
                },
            };

            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");
            PixelAssert.AlphaEquals(expectedValue, image);
        }

        [InlineData(  0, 73)]
        [InlineData(100, 32)]
        [InlineData(155, 37)]
        [InlineData(255, 47)]
        [Theory] public async Task SubSurfaceScatteringTextureTest(byte actualValue, byte expectedValue)
        {
            await using var graph = Graph();

            graph.PackInput = packInput;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/sss.png", actualValue);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");
            PixelAssert.AlphaEquals(expectedValue, image);
        }

        [InlineData(0.0, 73)]
        [InlineData(0.1, 50)]
        [InlineData(0.5, 60)]
        [InlineData(0.9, 70)]
        [InlineData(1.0, 72)]
        [Theory] public async Task EmissiveValueTest(decimal actualValue, byte expectedValue)
        {
            await using var graph = Graph();

            graph.PackInput = packInput;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
                Emissive = new MaterialEmissiveProperties {
                    Value = actualValue,
                },
            };

            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");
            PixelAssert.AlphaEquals(expectedValue, image);
        }

        [InlineData(  0, 73)]
        [InlineData(  8, 49)]
        [InlineData(100, 57)]
        [InlineData(155, 63)]
        [InlineData(255, 72)]
        [Theory] public async Task EmissiveTextureTest(byte actualValue, byte expectedValue)
        {
            await using var graph = Graph();

            graph.PackInput = packInput;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/emissive.png", actualValue);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");
            PixelAssert.AlphaEquals(expectedValue, image);
        }

        [InlineData(0.0,  73)]
        [InlineData(0.1,  81)]
        [InlineData(0.5, 115)]
        [InlineData(0.9, 149)]
        [InlineData(1.0, 157)]
        [Theory] public async Task SmoothValueTest(decimal actualValue, byte expectedValue)
        {
            await using var graph = Graph();

            graph.PackInput = packInput;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
                Smooth = new MaterialSmoothProperties {
                    Value = actualValue,
                },
            };

            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");
            PixelAssert.AlphaEquals(expectedValue, image);
        }

        [InlineData(  0,  73)]
        [InlineData(100, 106)]
        [InlineData(155, 124)]
        [InlineData(255, 157)]
        [Theory] public async Task SmoothTextureTest(byte actualValue, byte expectedValue)
        {
            await using var graph = Graph();

            graph.PackInput = packInput;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/smooth.png", actualValue);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");
            PixelAssert.AlphaEquals(expectedValue, image);
        }

        [Fact] public async Task PerPixelTest()
        {
            await using var graph = Graph();

            graph.PackInput = packInput;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync<L8>("assets/test/sss.png", 4, 1, image => {
                image[1, 0] = new L8(255);
            });

            await graph.CreateImageAsync<L8>("assets/test/emissive.png", 4, 1, image => {
                image[2, 0] = new L8(255);
            });

            await graph.CreateImageAsync<L8>("assets/test/specular.png", 4, 1, image => {
                image[3, 0] = new L8(255);
            });

            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");

            PixelAssert.AlphaEquals(255, image);
            PixelAssert.AlphaEquals( 21, image, 1);
            PixelAssert.AlphaEquals(128, image, 2);
            PixelAssert.AlphaEquals(251, image, 3);
        }

        [Fact] public async Task OpacityClipTest()
        {
            await using var graph = Graph();

            graph.PackInput = packInput;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync<L8>("assets/test/opacity.png", 9, 1, image => {
                image[1, 0] = new L8(255);
                image[3, 0] = new L8(255);
                image[5, 0] = new L8(255);
                image[7, 0] = new L8(255);
                image[8, 0] = new L8(255);
            });

            await graph.CreateImageAsync<L8>("assets/test/smooth.png", 9, 1, image => {
                image[0, 0] = new L8(255);
                image[1, 0] = new L8(255);
            });

            await graph.CreateImageAsync<L8>("assets/test/sss.png", 9, 1, image => {
                image[2, 0] = new L8(255);
                image[3, 0] = new L8(255);
            });

            await graph.CreateImageAsync<L8>("assets/test/emissive.png", 9, 1, image => {
                image[4, 0] = new L8(255);
                image[5, 0] = new L8(255);
            });

            await graph.CreateImageAsync<L8>("assets/test/metal.png", 9, 1, image => {
                image[6, 0] = new L8(255);
                image[7, 0] = new L8(255);
            });

            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test.png");

            // Smooth
            PixelAssert.AlphaEquals(  0, image);
            PixelAssert.AlphaEquals(157, image, 1);

            // SSS
            PixelAssert.AlphaEquals(  0, image, 2);
            PixelAssert.AlphaEquals( 47, image, 3);

            // Emissive
            PixelAssert.AlphaEquals(  0, image, 4);
            PixelAssert.AlphaEquals( 72, image, 5);

            // Metal
            PixelAssert.AlphaEquals(  0, image, 6);
            PixelAssert.AlphaEquals(220, image, 7);

            PixelAssert.AlphaEquals( 73, image, 8);
        }
    }
}
