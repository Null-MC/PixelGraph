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

public class OpacityTests : ImageTestBase
{
    private readonly ProjectData project;
    private readonly PublishProfileProperties packProfile;


    public OpacityTests(ITestOutputHelper output) : base(output)
    {
        Builder.ConfigureReader(ContentTypes.File, GameEditions.None, null);
        Builder.ConfigureWriter(ContentTypes.File, GameEditions.None, null);

        project = new ProjectData {
            Input = new PackInputEncoding {
                Opacity = new ResourcePackOpacityChannelProperties {
                    Texture = TextureTags.Opacity,
                    Color = ColorChannel.Red,
                },
            },
        };

        packProfile = new PublishProfileProperties {
            Encoding = new PackOutputEncoding {
                Opacity = new ResourcePackOpacityChannelProperties {
                    Texture = TextureTags.Color,
                    Color = ColorChannel.Alpha,
                },
            },
        };
    }

    [InlineData(0.00,   0)]
    [InlineData(0.40, 102)]
    [InlineData(0.60, 153)]
    [InlineData(1.00, 255)]
    [Theory] public async Task PassthroughValue(decimal actualValue, byte expectedValue)
    {
        await using var graph = DefaultGraph(project, packProfile);

        graph.Material!.Opacity = new MaterialOpacityProperties {
            Value = actualValue,
        };

        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<Rgba32>("assets/test.png");
        PixelAssert.AlphaEquals(expectedValue, image);
    }

    [InlineData(  0)]
    [InlineData(100)]
    [InlineData(155)]
    [InlineData(255)]
    [Theory] public async Task PassthroughTexture(byte value)
    {
        await using var graph = DefaultGraph(project, packProfile);

        await graph.CreateImageAsync("assets/test/opacity.png", value);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<Rgba32>("assets/test.png");
        PixelAssert.AlphaEquals(value, image);
    }

    //[InlineData(  0, 0.00,   0)]
    //[InlineData(100, 1.00, 100)]
    //[InlineData(100, 0.50,  50)]
    //[InlineData(100, 2.00, 200)]
    //[InlineData(100, 3.00, 255)]
    //[InlineData(200, 0.01,   2)]
    //[Theory] public async Task ScaleValue(decimal value, decimal scale, byte expected)
    //{
    //    await using var graph = Graph();

    //    graph.PackInput = packInput;
    //    graph.PackProfile = packProfile;
    //    graph.Material = new MaterialProperties {
    //        Name = "test",
    //        LocalPath = "assets",
    //        Alpha = new MaterialAlphaProperties {
    //            Value = value,
    //            Scale = scale,
    //        },
    //    };

    //    await graph.ProcessAsync();

    //    using var image = await graph.GetImageAsync("assets/test.png");
    //    PixelAssert.AlphaEquals(expected, image);
    //}

    [InlineData(  0, 0.0,   0)]
    [InlineData(100, 1.0, 100)]
    [InlineData(100, 0.5,  50)]
    [InlineData(100, 2.0, 200)]
    [InlineData(100, 3.0, 255)]
    [InlineData(200, 0.01,  2)]
    [Theory] public async Task ScaleTexture(byte value, decimal scale, byte expected)
    {
        await using var graph = DefaultGraph(project, packProfile);

        graph.Material!.Opacity = new MaterialOpacityProperties {
            Scale = scale,
        };

        await graph.CreateImageAsync("assets/test/opacity.png", value);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<Rgba32>("assets/test.png");
        PixelAssert.AlphaEquals(expected, image);
    }
}