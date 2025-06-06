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

public class MetalTests : ImageTestBase
{
    private readonly ProjectData project;
    private readonly PublishProfileProperties packProfile;


    public MetalTests(ITestOutputHelper output) : base(output)
    {
        Builder.ConfigureReader(ContentTypes.File, GameEditions.None, null);
        Builder.ConfigureWriter(ContentTypes.File, GameEditions.None, null);

        project = new ProjectData {
            Input = new PackInputEncoding {
                Metal = new ResourcePackMetalChannelProperties {
                    Texture = TextureTags.Metal,
                    Color = ColorChannel.Red,
                },
            },
        };

        packProfile = new PublishProfileProperties {
            Encoding = new PackOutputEncoding {
                Metal = new ResourcePackMetalChannelProperties {
                    Texture = TextureTags.Metal,
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

        await graph.CreateImageAsync("assets/test/metal.png", value);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<L8>("assets/test_metal.png");
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

        graph.Material!.Metal = new MaterialMetalProperties {
            Scale = scale,
        };

        await graph.CreateImageAsync("assets/test/metal.png", value);
        await graph.ProcessAsync();

        using var image = await graph.GetImageAsync<L8>("assets/test_metal.png");
        PixelAssert.Equals(expected, image);
    }

    [InlineData(  0,   0)]
    [InlineData(229,   0)]
    [InlineData(230, 255)]
    [InlineData(250, 255)]
    [InlineData(255, 255)]
    [Theory] public async Task ConvertsHcmToMetal(byte value, byte expected)
    {
        project.Input = new PackInputEncoding {
            HCM = new ResourcePackHcmChannelProperties {
                Texture = TextureTags.HCM,
                Color = ColorChannel.Red,
                MinValue = 230m,
                MaxValue = 255m,
                RangeMin = 230,
                RangeMax = 255,
                EnableClipping = true,
            },
        };

        await using var graph = DefaultGraph(project, packProfile);

        await graph.CreateImageAsync("assets/test/hcm.png", value);
        await graph.ProcessAsync();
            
        using var image = await graph.GetImageAsync<L8>("assets/test_metal.png");
        PixelAssert.Equals(expected, image);
    }
}