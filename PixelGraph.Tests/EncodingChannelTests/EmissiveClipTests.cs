using PixelGraph.Common;
using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.Tests.Internal;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.EncodingChannelTests
{
    public class EmissiveClipTests : ImageTestBase
    {
        private readonly ProjectData project;
        private readonly PublishProfileProperties packProfile;


        public EmissiveClipTests(ITestOutputHelper output) : base(output)
        {
            Builder.ConfigureReader(ContentTypes.File, GameEditions.None, null);
            Builder.ConfigureWriter(ContentTypes.File, GameEditions.None, null);

            project = new ProjectData {
                Input = new PackInputEncoding {
                    Emissive = {
                        Texture = TextureTags.Emissive,
                        Color = ColorChannel.Red,
                        //Shift = -1,
                    },
                },
            };

            packProfile = new PublishProfileProperties {
                Encoding = {
                    Emissive = {
                        Texture = TextureTags.Emissive,
                        Color = ColorChannel.Red,
                        //Shift = -1,
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
            await using var graph = Graph();

            graph.Project = project;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/emissive.png", value);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_e.png");
            PixelAssert.RedEquals(value, image);
        }

        //[InlineData(       0, 0.00, 255)]
        //[InlineData(       1, 0.00, 255)]
        //[InlineData(100/255d, 1.00,  99)]
        //[InlineData(100/255d, 0.50,  49)]
        //[InlineData(100/255d, 2.00, 199)]
        //[InlineData(100/255d, 3.00, 254)]
        //[InlineData(200/255d, 0.01,   1)]
        //[Theory] public async Task ScaleValue(decimal value, decimal scale, byte expected)
        //{
        //    await using var graph = Graph();

        //    graph.PackInput = packInput;
        //    graph.PackProfile = packProfile;
        //    graph.Material = new MaterialProperties {
        //        Name = "test",
        //        LocalPath = "assets",
        //        Emissive = new MaterialEmissiveProperties {
        //            Value = value,
        //            Scale = scale,
        //        },
        //    };

        //    await graph.ProcessAsync();

        //    using var image = await graph.GetImageAsync("assets/test_e.png");
        //    PixelAssert.RedEquals(expected, image);
        //}

        [InlineData(255, 0.00,   0)]
        [InlineData(  0, 0.00,   0)]
        [InlineData( 99, 1.00,  99)]
        [InlineData( 99, 0.50,  50)]
        [InlineData( 99, 2.00, 198)]
        [InlineData( 99, 3.00, 255)]
        [InlineData(199, 0.01,   2)]
        [Theory] public async Task ScaleTexture(byte value, decimal scale, byte expected)
        {
            await using var graph = Graph();

            graph.Project = project;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
                Emissive = new MaterialEmissiveProperties {
                    Scale = scale,
                },
            };

            await graph.CreateImageAsync("assets/test/emissive.png", value);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_e.png");
            PixelAssert.RedEquals(expected, image);
        }

        [InlineData(  0, 255)]
        [InlineData(  1,   0)]
        [InlineData(128, 127)]
        [InlineData(255, 254)]
        [Theory] public async Task ShiftDown(byte value, byte expected)
        {
            await using var graph = Graph();

            graph.Project = new ProjectData {
                Input = new PackInputEncoding {
                    Emissive = {
                        Texture = TextureTags.Emissive,
                        Color = ColorChannel.Red,
                    },
                },
            };

            graph.PackProfile = new PublishProfileProperties {
                Encoding = {
                    Emissive = {
                        Texture = TextureTags.Emissive,
                        Color = ColorChannel.Red,
                        Shift = -1,
                    },
                },
            };

            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/emissive.png", value);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_e.png");
            PixelAssert.RedEquals(expected, image);
        }

        [InlineData(  0,   1)]
        [InlineData(127, 128)]
        [InlineData(254, 255)]
        [InlineData(255,   0)]
        [Theory] public async Task ShiftUp(byte value, byte expected)
        {
            await using var graph = Graph();

            graph.Project = new ProjectData {
                Input = new PackInputEncoding {
                    Emissive = {
                        Texture = TextureTags.Emissive,
                        Color = ColorChannel.Red,
                    },
                },
            };

            graph.PackProfile = new PublishProfileProperties {
                Encoding = {
                    Emissive = {
                        Texture = TextureTags.Emissive,
                        Color = ColorChannel.Red,
                        Shift = 1,
                    },
                },
            };

            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/emissive.png", value);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_e.png");
            PixelAssert.RedEquals(expected, image);
        }
    }
}
