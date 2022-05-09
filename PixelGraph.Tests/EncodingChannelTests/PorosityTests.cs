using PixelGraph.Common;
using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.Tests.Internal;
using SixLabors.ImageSharp.PixelFormats;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.EncodingChannelTests
{
    public class PorosityTests : ImageTestBase
    {
        private readonly ProjectData project;
        private readonly PublishProfileProperties packProfile;


        public PorosityTests(ITestOutputHelper output) : base(output)
        {
            Builder.ConfigureReader(ContentTypes.File, GameEditions.None, null);
            Builder.ConfigureWriter(ContentTypes.File, GameEditions.None, null);

            project = new ProjectData {
                Input = new PackInputEncoding {
                    Porosity = {
                        Texture = TextureTags.Porosity,
                        Color = ColorChannel.Red,
                    },
                },
            };

            packProfile = new PublishProfileProperties {
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
            await using var graph = Graph();

            graph.Project = project;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/porosity.png", value);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync<L8>("assets/test_p.png");
            PixelAssert.Equals(value, image);
        }

        //[InlineData(       0, 0.00,   0)]
        //[InlineData(       1, 0.00,   0)]
        //[InlineData(100/255d, 1.00, 100)]
        //[InlineData(100/255d, 0.50,  50)]
        //[InlineData(100/255d, 2.00, 200)]
        //[InlineData(100/255d, 3.00, 255)]
        //[InlineData(200/255d, 0.01,   2)]
        //[Theory] public async Task ScaleValue(decimal value, decimal scale, byte expected)
        //{
        //    await using var graph = Graph();

        //    graph.PackInput = packInput;
        //    graph.PackProfile = packProfile;
        //    graph.Material = new MaterialProperties {
        //        Name = "test",
        //        LocalPath = "assets",
        //        Porosity = new MaterialPorosityProperties {
        //            Value = value,
        //            Scale = scale,
        //        },
        //    };

        //    await graph.ProcessAsync();

        //    using var image = await graph.GetImageAsync("assets/test_p.png");
        //    PixelAssert.RedEquals(expected, image);
        //}

        [InlineData(  0, 0.00,   0)]
        [InlineData(100, 1.00, 100)]
        [InlineData(100, 0.50,  50)]
        [InlineData(100, 2.00, 200)]
        [InlineData(100, 3.00, 255)]
        [InlineData(200, 0.01,   2)]
        [Theory] public async Task ScaleTexture(byte value, decimal scale, byte expected)
        {
            await using var graph = Graph();

            graph.Project = project;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
                Porosity = new MaterialPorosityProperties {
                    Scale = scale,
                },
            };

            await graph.CreateImageAsync("assets/test/porosity.png", value);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync<L8>("assets/test_p.png");
            PixelAssert.Equals(expected, image);
        }
    }
}
