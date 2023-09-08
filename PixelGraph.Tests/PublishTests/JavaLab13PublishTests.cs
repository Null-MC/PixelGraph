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

public class JavaLab13PublishTests : ImageTestBase
{
    private readonly ProjectData project;
    private readonly PublishProfileProperties packProfile;


    public JavaLab13PublishTests(ITestOutputHelper output) : base(output)
    {
        Builder.ConfigureReader(ContentTypes.File, GameEditions.None, null);
        Builder.ConfigureWriter(ContentTypes.File, GameEditions.Java, null);

        project = new ProjectData {
            Input = new PackInputEncoding {
                Format = TextureFormat.Format_Raw,
            },
        };

        packProfile = new PublishProfileProperties {
            Encoding = {
                Format = TextureFormat.Format_Lab13,
            },
        };
    }

    [InlineData(  0)]
    [InlineData(100)]
    [InlineData(155)]
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

        using var image = await graph.GetImageAsync<Rgba32>("assets/test.png");
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
        PixelAssert.RedEquals(red, image);
        PixelAssert.GreenEquals(green, image);
        PixelAssert.BlueEquals(blue, image);
    }

    [InlineData(new byte[]{127, 127, 255}, new byte[]{127, 127})]
    [InlineData(new byte[]{127,   0,   0}, new byte[]{127,   0})]
    [InlineData(new byte[]{  0, 127,   0}, new byte[]{  0, 127})]
    [Theory] public async Task NormalTextureTest(byte[] normalIn, byte[] normalOut)
    {
        await using var graph = Graph();

        graph.Project = project;
        graph.PackProfile = packProfile;
        graph.Material = new MaterialProperties {
            Name = "test",
            LocalPath = "assets",
        };

        await graph.CreateImageAsync("assets/test/normal.png", normalIn[0], normalIn[1], normalIn[2]);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<Rgb24>("assets/test_n.png");
        PixelAssert.RedEquals(normalOut[0], image);
        PixelAssert.GreenEquals(normalOut[1], image);
        //PixelAssert.BlueEquals(255, image);
    }

    [InlineData(  0)]
    [InlineData(100)]
    [InlineData(155)]
    [InlineData(255)]
    [Theory] public async Task OcclusionTextureTest(byte value)
    {
        await using var graph = Graph();

        graph.Project = project;
        graph.PackProfile = packProfile;
        graph.Material = new MaterialProperties {
            Name = "test",
            LocalPath = "assets",
        };

        await graph.CreateImageAsync("assets/test/occlusion.png", value);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<Rgba32>("assets/test_n.png");
        PixelAssert.BlueEquals(value, image);
    }

    [InlineData(  0)]
    [InlineData(100)]
    [InlineData(155)]
    [InlineData(255)]
    [Theory] public async Task HeightTextureTest(byte value)
    {
        await using var graph = Graph();

        graph.Project = project;
        graph.PackProfile = packProfile;
        graph.Material = new MaterialProperties {
            Name = "test",
            LocalPath = "assets",
        };

        await graph.CreateImageAsync("assets/test/height.png", value);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<Rgba32>("assets/test_n.png");
        PixelAssert.AlphaEquals(value, image);
    }

    [InlineData(  0)]
    [InlineData(100)]
    [InlineData(155)]
    [InlineData(255)]
    [Theory] public async Task SmoothTextureTest(byte value)
    {
        await using var graph = Graph();

        graph.Project = project;
        graph.PackProfile = packProfile;
        graph.Material = new MaterialProperties {
            Name = "test",
            LocalPath = "assets",
        };

        await graph.CreateImageAsync("assets/test/smooth.png", value);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<Rgba32>("assets/test_s.png");
        PixelAssert.RedEquals(value, image);
    }

    [InlineData(  0,  10)]
    [InlineData(  6,   6)]
    [InlineData(100, 100)]
    [InlineData(155, 155)]
    [InlineData(229, 229)]
    [InlineData(230,  10)]
    [InlineData(255,  10)]
    [Theory] public async Task F0TextureTest(byte actualValue, byte expectedValue)
    {
        await using var graph = Graph();

        graph.Project = project;
        graph.PackProfile = packProfile;
        graph.Material = new MaterialProperties {
            Name = "test",
            LocalPath = "assets",
        };

        await graph.CreateImageAsync("assets/test/f0.png", actualValue);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<Rgba32>("assets/test_s.png");
        PixelAssert.GreenEquals(expectedValue, image);
    }

    [InlineData(  0,  10)]
    [InlineData(229,  10)]
    [InlineData(230, 230)]
    [InlineData(231, 231)]
    [InlineData(240, 240)]
    [InlineData(255, 255)]
    [Theory] public async Task HCMTextureTest(byte inputValue, byte outputValue)
    {
        await using var graph = Graph();

        graph.Project = project;
        graph.PackProfile = packProfile;
        graph.Material = new MaterialProperties {
            Name = "test",
            LocalPath = "assets",
        };

        await graph.CreateImageAsync("assets/test/hcm.png", inputValue);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<Rgba32>("assets/test_s.png");
        PixelAssert.GreenEquals(outputValue, image);
    }

    [InlineData(  0,  10)]
    [InlineData(229,  10)]
    [InlineData(230, 230)]
    [InlineData(231, 231)]
    [InlineData(240, 240)]
    [InlineData(255, 255)]
    [Theory] public async Task HCMValueTest(byte inputValue, byte outputValue)
    {
        await using var graph = Graph();

        graph.Project = project;
        graph.PackProfile = packProfile;
        graph.Material = new MaterialProperties {
            Name = "test",
            LocalPath = "assets",
            HCM = {
                Value = inputValue,
            }
        };

        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<Rgba32>("assets/test_s.png");
        PixelAssert.GreenEquals(outputValue, image);
    }

    [InlineData(  0,  0)]
    [InlineData(100, 25)]
    [InlineData(155, 39)]
    [InlineData(255, 64)]
    [Theory] public async Task PorosityTextureTest(byte actualValue, byte expectedValue)
    {
        await using var graph = Graph();

        graph.Project = project;
        graph.PackProfile = packProfile;
        graph.Material = new MaterialProperties {
            Name = "test",
            LocalPath = "assets",
        };

        await graph.CreateImageAsync("assets/test/porosity.png", actualValue);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<Rgba32>("assets/test_s.png");
        PixelAssert.BlueEquals(expectedValue, image);
    }

    [InlineData(  0,  0)]
    [InlineData(100, 140)]
    [InlineData(155, 180)]
    [InlineData(255, 255)]
    [Theory] public async Task SubSurfaceScatteringTextureTest(byte actualValue, byte expectedValue)
    {
        await using var graph = Graph();

        graph.Project = project;
        graph.PackProfile = packProfile;
        graph.Material = new MaterialProperties {
            Name = "test",
            LocalPath = "assets",
        };

        await graph.CreateImageAsync("assets/test/sss.png", actualValue);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<Rgba32>("assets/test_s.png");
        PixelAssert.BlueEquals(expectedValue, image);
    }

    [InlineData(  0, 255)]
    [InlineData(  1,   0)]
    [InlineData(100,  99)]
    [InlineData(155, 154)]
    [InlineData(255, 254)]
    [Theory] public async Task EmissiveTextureTest(byte actualValue, byte expectedValue)
    {
        await using var graph = Graph();

        graph.Project = project;
        graph.PackProfile = packProfile;
        graph.Material = new MaterialProperties {
            Name = "test",
            LocalPath = "assets",
        };

        await graph.CreateImageAsync("assets/test/emissive.png", actualValue);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<Rgba32>("assets/test_s.png");
        PixelAssert.AlphaEquals(expectedValue, image);
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

        await graph.CreateImageAsync<L8>("assets/test/smooth.png", 7, 1, image => {
            image[1, 0] = new L8(200);
        });

        await graph.CreateImageAsync<L8>("assets/test/f0.png", 7, 1, image => {
            image[2, 0] = new L8(10);
        });

        await graph.CreateImageAsync<L8>("assets/test/hcm.png", 7, 1, image => {
            image[3, 0] = new L8(231);
        });

        await graph.CreateImageAsync<L8>("assets/test/porosity.png", 7, 1, image => {
            image[4, 0] = new L8(128);
        });

        await graph.CreateImageAsync<L8>("assets/test/sss.png", 7, 1, image => {
            image[5, 0] = new L8(128);
        });

        await graph.CreateImageAsync<L8>("assets/test/emissive.png", 7, 1, image => {
            image[6, 0] = new L8(100);
        });

        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<Rgba32>("assets/test_s.png");

        PixelAssert.Equals(  0,  10,   0, 255, image);
        PixelAssert.Equals(200,  10,   0, 255, image, 1);
        PixelAssert.Equals(  0,  10,   0, 255, image, 2);
        PixelAssert.Equals(  0, 231,   0, 255, image, 3);
        PixelAssert.Equals(  0,  10,  32, 255, image, 4);
        PixelAssert.Equals(  0,  10, 160, 255, image, 5);
        PixelAssert.Equals(  0,  10,   0,  99, image, 6);
    }
}