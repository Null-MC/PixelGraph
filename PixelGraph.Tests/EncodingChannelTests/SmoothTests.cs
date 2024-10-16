﻿using PixelGraph.Common;
using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.Tests.Internal;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.EncodingChannelTests;

public class SmoothTests : ImageTestBase
{
    private readonly ProjectData project;
    private readonly PublishProfileProperties packProfile;


    public SmoothTests(ITestOutputHelper output) : base(output)
    {
        Builder.ConfigureReader(ContentTypes.File, GameEditions.None, null);
        Builder.ConfigureWriter(ContentTypes.File, GameEditions.None, null);

        project = new ProjectData {
            Input = new PackInputEncoding {
                Smooth = new ResourcePackSmoothChannelProperties {
                    Texture = TextureTags.Smooth,
                    Color = ColorChannel.Red,
                },
            },
        };

        packProfile = new PublishProfileProperties {
            Encoding = new PackOutputEncoding {
                Smooth = new ResourcePackSmoothChannelProperties {
                    Texture = TextureTags.Smooth,
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
        await using var graph = DefaultGraph(project, packProfile);

        await graph.CreateImageAsync("assets/test/smooth.png", value);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<L8>("assets/test_smooth.png");
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
    //        Smooth = new MaterialSmoothProperties {
    //            Value = value,
    //            Scale = scale,
    //        },
    //    };

    //    await graph.ProcessAsync();

    //    using var image = await graph.GetImageAsync("assets/test_smooth.png");
    //    PixelAssert.RedEquals(expected, image);
    //}

    [InlineData(  0,  0.0,   0)]
    [InlineData(100,  1.0, 100)]
    [InlineData(100,  0.5,  50)]
    [InlineData(100,  2.0, 200)]
    [InlineData(100,  3.0, 255)]
    [InlineData(200, 0.01,   2)]
    [Theory] public async Task ScaleTexture(byte value, decimal scale, byte expected)
    {
        await using var graph = DefaultGraph(project, packProfile);

        graph.Material!.Smooth = new MaterialSmoothProperties {
            Scale = scale,
        };

        await graph.CreateImageAsync("assets/test/smooth.png", value);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<L8>("assets/test_smooth.png");
        PixelAssert.Equals(expected, image);
    }

    [InlineData(  0, 255)]
    [InlineData(100, 155)]
    [InlineData(155, 100)]
    [InlineData(255,   0)]
    [Theory] public async Task ConvertsRoughToSmooth(byte value, byte expected)
    {
        project.Input = new PackInputEncoding {
            Rough = new ResourcePackRoughChannelProperties {
                Texture = TextureTags.Rough,
                Color = ColorChannel.Red,
            },
        };

        await using var graph = DefaultGraph(project, packProfile);

        await graph.CreateImageAsync("assets/test/rough.png", value);
        await graph.ProcessAsync();
            
        using var image = await graph.GetImageAsync<L8>("assets/test_smooth.png");
        PixelAssert.Equals(expected, image);
    }

    //[InlineData(  0,   0)]
    //[InlineData(160, 220)]
    //[InlineData(226, 252)]
    //[InlineData(255, 255)]
    //[Theory] public async Task ConvertsPerceptualSmoothToSmooth(byte value, byte expected)
    //{
    //    var context = new MaterialContext {
    //        Input = new ResourcePackInputProperties {
    //            Smooth = {
    //                Texture = TextureTags.Smooth,
    //                Color = ColorChannel.Red,
    //                Perceptual = true,
    //            },
    //        },
    //        Profile = packProfile,
    //        Material = new MaterialProperties {
    //            Name = "test",
    //            LocalPath = "assets",
    //        },
    //    };

    //    await using var graph = Graph(context);
    //    await graph.CreateImageAsync("assets/test/smooth.png", value, 0, 0);
    //    await graph.ProcessAsync();
            
    //    using var image = await graph.GetImageAsync("assets/test_smooth.png");
    //    PixelAssert.RedEquals(expected, image);
    //}
}