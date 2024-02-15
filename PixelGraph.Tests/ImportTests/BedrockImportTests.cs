using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO.Importing;
using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Tests.Internal;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.ImportTests;

public class BedrockImportTests : ImageTestBase
{
    private readonly ProjectData project;
    private readonly PublishProfileProperties packProfile;


    public BedrockImportTests(ITestOutputHelper output) : base(output)
    {
        Builder.ConfigureReader(ContentTypes.File, GameEditions.Bedrock, null);
        Builder.ConfigureWriter(ContentTypes.File, GameEditions.None, null);
        Builder.AddImporter(GameEditions.Bedrock);

        project = new ProjectData {
            Input = new PackInputEncoding {
                Format = TextureFormat.Format_Raw,
            },
        };

        packProfile = new PublishProfileProperties {
            Encoding = new PackOutputEncoding {
                Format = TextureFormat.Format_Color,
            },
        };
    }

    [Fact]
    public async Task CanImportLocal()
    {
        await using var graph = Graph();

        await graph.CreateImageAsync("textures/blocks/brick.png", 31, 156, 248, 250);

        var importer = graph.Provider.GetRequiredService<IMaterialImporter>();

        importer.Project = project;
        importer.PackProfile = packProfile;
        importer.AsGlobal = false;

        var material = new MaterialProperties {
            Name = "brick",
            LocalPath = PathEx.Localize("textures/blocks"),
            UseGlobalMatching = true,
        };

        await importer.ImportAsync(material);

        using var colorImage = await graph.GetImageAsync<Rgb24>("assets/minecraft/textures/block/bricks/color.png");
        PixelAssert.Equals(31, 156, 248, colorImage);

        using var opacityImage = await graph.GetImageAsync<L8>("assets/minecraft/textures/block/bricks/opacity.png");
        PixelAssert.Equals(250, opacityImage);
    }
}