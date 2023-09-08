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

namespace PixelGraph.Tests.EncodingChannelTests;

public class EmissiveTests : ImageTestBase
{
    private readonly ProjectData project;
    private readonly PublishProfileProperties packProfile;


    public EmissiveTests(ITestOutputHelper output) : base(output)
    {
        Builder.ConfigureReader(ContentTypes.File, GameEditions.None, null);
        Builder.ConfigureWriter(ContentTypes.File, GameEditions.None, null);

        project = new ProjectData {
            Input = new PackInputEncoding {
                Emissive = {
                    Texture = TextureTags.Emissive,
                    Color = ColorChannel.Red,
                },
            },
        };

        packProfile = new PublishProfileProperties {
            Encoding = {
                Emissive = {
                    Texture = TextureTags.Emissive,
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

        await graph.CreateImageAsync("assets/test/emissive.png", value);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<L8>("assets/test_e.png");
        PixelAssert.Equals(value, image);
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
    //        Emissive = new MaterialEmissiveProperties {
    //            Value = value,
    //            Scale = scale,
    //        },
    //    };

    //    await graph.ProcessAsync();

    //    using var image = await graph.GetImageAsync("assets/test_e.png");
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
            Emissive = new MaterialEmissiveProperties {
                Scale = scale,
            },
        };

        await graph.CreateImageAsync("assets/test/emissive.png", value);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<L8>("assets/test_e.png");
        PixelAssert.Equals(expected, image);
    }

    [InlineData(  0,   1)]
    [InlineData(127, 128)]
    [InlineData(254, 255)]
    [InlineData(255,   0)]
    [Theory] public async Task ConvertsEmissiveClippedToEmissive(byte value, byte expected)
    {
        await using var graph = Graph();

        graph.Project = new ProjectData {
            Input = new PackInputEncoding {
                Emissive = {
                    Texture = TextureTags.Emissive,
                    Color = ColorChannel.Red,
                    Shift = -1,
                },
            },
        };

        graph.PackProfile = packProfile;
        graph.Material = new MaterialProperties {
            Name = "test",
            LocalPath = "assets",
        };

        await graph.CreateImageAsync("assets/test/emissive.png", value);
        await graph.ProcessAsync();
            
        using var image = await graph.GetImageAsync<L8>("assets/test_e.png");
        PixelAssert.Equals(expected, image);
    }

    [InlineData(0, 255)]
    [InlineData(128, 127)]
    [InlineData(254, 1)]
    [InlineData(255, 0)]
    [Theory] public async Task ConvertsEmissiveInverseToEmissive(byte value, byte expected)
    {
        await using var graph = Graph();

        graph.Project = new ProjectData {
            Input = new PackInputEncoding {
                Emissive = {
                    Texture = TextureTags.Emissive,
                    Color = ColorChannel.Red,
                    Invert = true,
                },
            },
        };

        graph.PackProfile = packProfile;
        graph.Material = new MaterialProperties {
            Name = "test",
            LocalPath = "assets",
        };

        await graph.CreateImageAsync("assets/test/emissive.png", value);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<L8>("assets/test_e.png");
        PixelAssert.Equals(expected, image);
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

        using var image = await graph.GetImageAsync<L8>("assets/test_e.png");
        PixelAssert.Equals(expected, image);
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

        using var image = await graph.GetImageAsync<L8>("assets/test_e.png");
        PixelAssert.Equals(expected, image);
    }
}