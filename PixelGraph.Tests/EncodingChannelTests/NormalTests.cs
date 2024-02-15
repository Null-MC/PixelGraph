using PixelGraph.Common;
using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.Tests.Internal;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.EncodingChannelTests;

public class NormalTests : ImageTestBase
{
    private readonly PublishProfileProperties packProfile;


    public NormalTests(ITestOutputHelper output) : base(output)
    {
        Builder.ConfigureReader(ContentTypes.File, GameEditions.None, null);
        Builder.ConfigureWriter(ContentTypes.File, GameEditions.None, null);

        packProfile = new PublishProfileProperties {
            Encoding = new PackOutputEncoding {
                NormalX = new ResourcePackNormalXChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Red,
                    MinValue = -1m,
                    MaxValue = 1m,
                    DefaultValue = 0m,
                },
                NormalY = new ResourcePackNormalYChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Green,
                    MinValue = -1m,
                    MaxValue = 1m,
                    DefaultValue = 0m,
                },
                NormalZ = new ResourcePackNormalZChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Blue,
                    MinValue = -1m,
                    MaxValue = 1m,
                    DefaultValue = 1m,
                },
            },
        };
    }

    [InlineData(127, 127, 255)]
    [InlineData(  0, 127,   0)]
    [InlineData(255, 127,   0)]
    [InlineData(127,   0,   0)]
    [InlineData(127, 255,   0)]
    [Theory] public async Task Passthrough(byte valueX, byte valueY, byte valueZ)
    {
        await using var graph = Graph();

        graph.Project = new ProjectData {
            Input = new PackInputEncoding {
                NormalX = new ResourcePackNormalXChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Red,
                    MinValue = -1m,
                    MaxValue = 1m,
                    DefaultValue = 0m,
                },
                NormalY = new ResourcePackNormalYChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Green,
                    MinValue = -1m,
                    MaxValue = 1m,
                    DefaultValue = 0m,
                },
                NormalZ = new ResourcePackNormalZChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Blue,
                    MinValue = -1m,
                    MaxValue = 1m,
                    DefaultValue = 1m,
                },
            },
        };
            
        graph.PackProfile = packProfile;
        graph.Material = new MaterialProperties {
            Name = "test",
            LocalPath = "assets",
        };

        await graph.CreateImageAsync("assets/test/normal.png", valueX, valueY, valueZ);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<Rgb24>("assets/test_n.png");
        PixelAssert.Equals(valueX, valueY, valueZ, image);
    }

    [InlineData(127, 127, 255)]
    [InlineData( 64, 127, 222)]
    [InlineData(127,  64, 222)]
    [Theory] public async Task RestoreZ(byte valueX, byte valueY, byte expectedZ)
    {
        await using var graph = Graph();

        graph.Project = new ProjectData {
            Input = new PackInputEncoding {
                NormalX = new ResourcePackNormalXChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Red,
                    MinValue = -1m,
                    MaxValue = 1m,
                    DefaultValue = 0m,
                },
                NormalY = new ResourcePackNormalYChannelProperties {
                    Texture = TextureTags.Normal,
                    Color = ColorChannel.Green,
                    MinValue = -1m,
                    MaxValue = 1m,
                    DefaultValue = 0m,
                },
            },
        };

        graph.PackProfile = packProfile;
        graph.Material = new MaterialProperties {
            Name = "test",
            LocalPath = "assets",
        };

        await graph.CreateImageAsync("assets/test/normal.png", valueX, valueY, 0);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<Rgb24>("assets/test_n.png");
        PixelAssert.Equals(valueX, valueY, expectedZ, image);
    }
}