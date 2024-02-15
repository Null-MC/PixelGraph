using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Importing;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Tests.Internal;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.ImportTests;

public class JavaOldPbrImportTests : ImageTestBase
{
    private readonly ProjectData project;
    private readonly PublishProfileProperties packProfile;


    public JavaOldPbrImportTests(ITestOutputHelper output) : base(output)
    {
        Builder.ConfigureReader(ContentTypes.File, GameEditions.Java, null);
        Builder.ConfigureWriter(ContentTypes.File, GameEditions.None, null);
        Builder.AddImporter(GameEditions.Java);

        project = new ProjectData {
            Input = new PackInputEncoding {
                Edition = GameEdition.Java,
                Format = TextureFormat.Format_Raw,
            },
        };

        packProfile = new PublishProfileProperties {
            Edition = GameEdition.Java,
            Encoding = new PackOutputEncoding {
                Format = TextureFormat.Format_OldPbr,
            },
        };
    }

    [Fact]
    public async Task CanImportLocal()
    {
        await using var graph = Graph();

        await graph.CreateImageAsync("assets/minecraft/textures/block/bricks.png", 31, 156, 248);
        await graph.CreateImageAsync("assets/minecraft/textures/block/bricks_n.png", 127, 127, 255, 250);
        await graph.CreateImageAsync("assets/minecraft/textures/block/bricks_s.png", 16, 0, 0);

        var importer = graph.Provider.GetRequiredService<IMaterialImporter>();
        importer.Project = project;
        importer.PackProfile = packProfile;
        importer.AsGlobal = false;

        var localPath = PathEx.Localize("assets/minecraft/textures/block");
        var srcMaterial = await importer.CreateMaterialAsync(localPath, "bricks");
        await importer.ImportAsync(srcMaterial);

        await using var materialFile = graph.GetFile("assets/minecraft/textures/block/bricks/mat.yml");
        Assert.NotNull(materialFile);

        using var colorImage = await graph.GetImageAsync<Rgb24>("assets/minecraft/textures/block/bricks/color.png");
        PixelAssert.Equals(31, 156, 248, colorImage);

        using var normalImage = await graph.GetImageAsync<Rgb24>("assets/minecraft/textures/block/bricks/normal.png");
        PixelAssert.Equals(127, 127, 255, normalImage);

        using var heightImage = await graph.GetImageAsync<L8>("assets/minecraft/textures/block/bricks/height.png");
        PixelAssert.Equals(250, heightImage);

        using var smoothImage = await graph.GetImageAsync<L8>("assets/minecraft/textures/block/bricks/smooth.png");
        PixelAssert.Equals(16, smoothImage);

        using var metalImage = await graph.GetImageAsync<L8>("assets/minecraft/textures/block/bricks/metal.png");
        PixelAssert.Equals(0, metalImage);

        using var emissiveImage = await graph.GetImageAsync<L8>("assets/minecraft/textures/block/bricks/emissive.png");
        PixelAssert.Equals(0, emissiveImage);
    }
}