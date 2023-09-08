using PixelGraph.Common;
using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Tests.Internal;
using SixLabors.ImageSharp.PixelFormats;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.PublishTests;

public class BedrockRtxPublishTests : ImageTestBase
{
    private readonly ProjectData project;
    private readonly PublishProfileProperties packProfile;


    public BedrockRtxPublishTests(ITestOutputHelper output) : base(output)
    {
        Builder.ConfigureReader(ContentTypes.File, GameEditions.None, null);
        Builder.ConfigureWriter(ContentTypes.File, GameEditions.Bedrock, null);

        project = new ProjectData {
            Input = new PackInputEncoding {
                Format = TextureFormat.Format_Raw,
            },
        };

        packProfile = new PublishProfileProperties {
            Encoding = {
                Format = TextureFormat.Format_Rtx,
            },
        };
    }

    [InlineData(  0)]
    [InlineData(  1)]
    [InlineData(127)]
    [InlineData(254)]
    [InlineData(255)]
    [Theory] public async Task OpacityTextureTest(byte value)
    {
        await using var graph = Graph();

        graph.Project = project;
        graph.PackProfile = packProfile;
        graph.Material = new MaterialProperties {
            Name = "test",
            LocalPath = "assets",
        };

        await graph.CreateImageAsync("assets/test/opacity.png", value);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync("assets/test.png");
        PixelAssert.AlphaEquals(value, image);
    }

    [InlineData(  0,   0,  0)]
    [InlineData(100, 101, 102)]
    [InlineData(155, 160, 165)]
    [InlineData(255, 255, 255)]
    [Theory] public async Task ColorTextureTest(byte red, byte green, byte blue)
    {
        await using var graph = Graph();

        graph.Project = project;
        graph.PackProfile = packProfile;
        graph.Material = new MaterialProperties {
            Name = "test",
            LocalPath = "assets",
        };

        await graph.CreateImageAsync("assets/test/color.png", red, green, blue);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<Rgb24>("assets/test.png");
        PixelAssert.Equals(red, green, blue, image);
    }

    [InlineData(127, 127, 255)]
    [InlineData(127,   0,   0)]
    [InlineData(127, 255,   0)]
    [InlineData(  0, 127,   0)]
    [InlineData(255, 127,   0)]
    [Theory] public async Task NormalTextureTest(byte red, byte green, byte blue)
    {
        await using var graph = Graph();

        graph.Project = project;
        graph.PackProfile = packProfile;
        graph.Material = new MaterialProperties {
            Name = "test",
            LocalPath = "assets",
        };

        await graph.CreateImageAsync("assets/test/normal.png", red, green, blue);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<Rgb24>("assets/test_normal.png");
        PixelAssert.Equals(red, green, blue, image);
    }

    //[InlineData(  0)]
    //[InlineData(100)]
    //[InlineData(155)]
    //[InlineData(255)]
    //[Theory] public async Task HeightTextureTest(byte value)
    //{
    //    await using var graph = Graph();

    //    graph.PackInput = packInput;
    //    graph.PackProfile = packProfile;
    //    graph.Material = new MaterialProperties {
    //        Name = "test",
    //        LocalPath = "assets",
    //    };

    //    await graph.CreateImageAsync("assets/test/height.png", value);
    //    await graph.ProcessAsync();

    //    using var image = await graph.GetImageAsync("assets/test_n.png");
    //    PixelAssert.AlphaEquals(value, image);
    //}

    [InlineData(  0)]
    [InlineData(  1)]
    [InlineData(127)]
    [InlineData(254)]
    [InlineData(255)]
    [Theory] public async Task MetalTextureTest(byte value)
    {
        await using var graph = Graph();

        graph.Project = project;
        graph.PackProfile = packProfile;
        graph.Material = new MaterialProperties {
            Name = "test",
            LocalPath = "assets",
        };

        await graph.CreateImageAsync("assets/test/metal.png", value);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<Rgb24>("assets/test_mer.png");
        PixelAssert.RedEquals(value, image);
    }

    [InlineData(0.0,   0)]
    [InlineData(0.1,  26)]
    [InlineData(0.5, 128)]
    [InlineData(0.9, 230)]
    [InlineData(1.0, 255)]
    [Theory] public async Task MetalValueTest(decimal inputValue, byte outputValue)
    {
        await using var graph = Graph();

        graph.Project = project;
        graph.PackProfile = packProfile;
        graph.Material = new MaterialProperties {
            Name = "test",
            LocalPath = "assets",
            Metal = {
                Value = inputValue,
            }
        };

        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<Rgb24>("assets/test_mer.png");
        PixelAssert.RedEquals(outputValue, image);
    }

    [InlineData(  0)]
    [InlineData(  1)]
    [InlineData(127)]
    [InlineData(254)]
    [InlineData(255)]
    [Theory] public async Task EmissiveTextureTest(byte value)
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

        using var image = await graph.GetImageAsync<Rgb24>("assets/test_mer.png");
        PixelAssert.GreenEquals(value, image);
    }

    [InlineData(  0)]
    [InlineData(  1)]
    [InlineData(127)]
    [InlineData(254)]
    [InlineData(255)]
    [Theory] public async Task RoughTextureTest(byte value)
    {
        await using var graph = Graph();

        graph.Project = project;
        graph.PackProfile = packProfile;
        graph.Material = new MaterialProperties {
            Name = "test",
            LocalPath = "assets",
        };

        await graph.CreateImageAsync("assets/test/rough.png", value);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<Rgb24>("assets/test_mer.png");
        PixelAssert.BlueEquals(value, image);
    }

    [Fact] public async Task PerPixelSpecularTextureTest()
    {
        await using var graph = Graph();

        graph.Project = project;
        graph.PackProfile = packProfile;
        graph.Material = new MaterialProperties {
            Name = "test",
            LocalPath = "assets",
        };

        await graph.CreateImageAsync<L8>("assets/test/metal.png", 4, 1, image => {
            image[1, 0] = new L8(240);
        });

        await graph.CreateImageAsync<L8>("assets/test/emissive.png", 4, 1, image => {
            image[2, 0] = new L8(100);
        });

        await graph.CreateImageAsync<L8>("assets/test/rough.png", 4, 1, image => {
            image[3, 0] = new L8(200);
        });

        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<Rgb24>("assets/test_mer.png");

        PixelAssert.Equals(  0,   0,   0, image);
        PixelAssert.Equals(240,   0,   0, image, 1);
        PixelAssert.Equals(  0, 100,   0, image, 2);
        PixelAssert.Equals(  0,   0, 200, image, 3);
    }
}