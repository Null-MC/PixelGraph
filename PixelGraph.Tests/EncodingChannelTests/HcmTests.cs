using PixelGraph.Common;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.Tests.Internal;
using System.Threading.Tasks;
using PixelGraph.Common.Projects;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.EncodingChannelTests
{
    public class HcmTests : ImageTestBase
    {
        private readonly ProjectData project;
        private readonly PublishProfileProperties packProfile;


        public HcmTests(ITestOutputHelper output) : base(output)
        {
            Builder.ConfigureReader(ContentTypes.File, GameEditions.None, null);
            Builder.ConfigureWriter(ContentTypes.File, GameEditions.None, null);

            project = new ProjectData {
                Input = new PackInputEncoding {
                    HCM = {
                        Texture = TextureTags.HCM,
                        Color = ColorChannel.Red,
                        MinValue = 230m,
                        MaxValue = 255m,
                        RangeMin = 230,
                        RangeMax = 255,
                        EnableClipping = true,
                    },
                },
            };

            packProfile = new PublishProfileProperties {
                Encoding = {
                    HCM = {
                        Texture = TextureTags.HCM,
                        Color = ColorChannel.Red,
                        MinValue = 230m,
                        MaxValue = 255m,
                        RangeMin = 230,
                        RangeMax = 255,
                        EnableClipping = true,
                    },
                },
            };
        }

        [InlineData(  0,   0)]
        [InlineData(229,   0)]
        [InlineData(230, 230)]
        [InlineData(250, 250)]
        [InlineData(255, 255)]
        [Theory] public async Task Passthrough(byte actualValue, byte expectedValue)
        {
            await using var graph = Graph();

            graph.Project = project;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/hcm.png", actualValue, 0, 0);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_hcm.png");
            PixelAssert.RedEquals(expectedValue, image);
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
        //        Smooth = new MaterialSmoothProperties {
        //            Value = value,
        //            Scale = scale,
        //        },
        //    };

        //    await graph.ProcessAsync();

        //    using var image = await graph.GetImageAsync("assets/test_smooth.png");
        //    PixelAssert.RedEquals(expected, image);
        //}

        //[InlineData(  0,  0.0,   0)]
        //[InlineData(100,  1.0, 100)]
        //[InlineData(100,  0.5,  50)]
        //[InlineData(100,  2.0, 200)]
        //[InlineData(100,  3.0, 255)]
        //[InlineData(200, 0.01,   2)]
        //[Theory] public async Task ScaleTexture(byte value, decimal scale, byte expected)
        //{
        //    await using var graph = Graph();

        //    graph.PackInput = packInput;
        //    graph.PackProfile = packProfile;
        //    graph.Material = new MaterialProperties {
        //        Name = "test",
        //        LocalPath = "assets",
        //        HCM = new MaterialHcmProperties {
        //            Scale = scale,
        //        },
        //    };

        //    await graph.CreateImageAsync("assets/test/metal.png", value, 0, 0);
        //    await graph.ProcessAsync();

        //    using var image = await graph.GetImageAsync("assets/test_metal.png");
        //    PixelAssert.RedEquals(expected, image);
        //}

        [InlineData(  0, 0)]
        [InlineData(100, 0)]
        [InlineData(155, 255)]
        [InlineData(255, 255)]
        [Theory] public async Task ConvertsMetalToHcm(byte value, byte expected)
        {
            await using var graph = Graph();

            graph.Project = new ProjectData {
                Input = new PackInputEncoding {
                    Metal = {
                        Texture = TextureTags.Metal,
                        Color = ColorChannel.Red,
                    },
                },
            };

            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/metal.png", value, 0, 0);
            await graph.ProcessAsync();
            
            using var image = await graph.GetImageAsync("assets/test_hcm.png");
            PixelAssert.RedEquals(expected, image);
        }
    }
}
