using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixelGraph.Common;
using PixelGraph.Common.Bedrock;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO.Importing;
using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Tests.Internal;
using SixLabors.ImageSharp.PixelFormats;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.ImportTests;

public class BedrockRtxImportTests : ImageTestBase
{
    private readonly ProjectData project;
    private readonly PublishProfileProperties packProfile;


    public BedrockRtxImportTests(ITestOutputHelper output) : base(output)
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
            Encoding = {
                Format = TextureFormat.Format_Rtx,
            },
        };
    }

    [Fact]
    public async Task CanImportLocal_WithNormal()
    {
        await using var graph = Graph();

        await graph.CreateImageAsync("textures/blocks/brick.png", 31, 156, 248);
        await graph.CreateImageAsync("textures/blocks/brick_normal.png", 127, 127, 200);
        await graph.CreateImageAsync("textures/blocks/brick_mer.png", 16, 8, 45);

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

        using var albedoImage = await graph.GetImageAsync<Rgb24>("assets/minecraft/textures/block/bricks/color.png");
        PixelAssert.Equals(31, 156, 248, albedoImage);

        using var opacityImage = await graph.GetImageAsync<L8>("assets/minecraft/textures/block/bricks/opacity.png");
        PixelAssert.Equals(255, opacityImage);

        using var normalImage = await graph.GetImageAsync<Rgb24>("assets/minecraft/textures/block/bricks/normal.png");
        PixelAssert.Equals(127, 127, 255, normalImage);

        using var metalImage = await graph.GetImageAsync<L8>("assets/minecraft/textures/block/bricks/metal.png");
        PixelAssert.Equals(16, metalImage);

        using var emissiveImage = await graph.GetImageAsync<L8>("assets/minecraft/textures/block/bricks/emissive.png");
        PixelAssert.Equals(8, emissiveImage);

        using var roughImage = await graph.GetImageAsync<L8>("assets/minecraft/textures/block/bricks/rough.png");
        PixelAssert.Equals(45, roughImage);
    }

    [Fact]
    public async Task CanImportLocal_WithHeight()
    {
        await using var graph = Graph();

        await graph.CreateImageAsync("textures/blocks/brick.png", 31, 156, 248);
        await graph.CreateImageAsync("textures/blocks/brick_heightmap.png", 252);
        await graph.CreateImageAsync("textures/blocks/brick_mer.png", 16, 8, 45);

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

        using var albedoImage = await graph.GetImageAsync<Rgb24>("assets/minecraft/textures/block/bricks/color.png");
        PixelAssert.Equals(31, 156, 248, albedoImage);

        using var opacityImage = await graph.GetImageAsync<L8>("assets/minecraft/textures/block/bricks/opacity.png");
        PixelAssert.Equals(255, opacityImage);

        using var heightImage = await graph.GetImageAsync<L8>("assets/minecraft/textures/block/bricks/height.png");
        PixelAssert.Equals(252, heightImage);

        using var metalImage = await graph.GetImageAsync<L8>("assets/minecraft/textures/block/bricks/metal.png");
        PixelAssert.Equals(16, metalImage);

        using var emissiveImage = await graph.GetImageAsync<L8>("assets/minecraft/textures/block/bricks/emissive.png");
        PixelAssert.Equals(8, emissiveImage);

        using var roughImage = await graph.GetImageAsync<L8>("assets/minecraft/textures/block/bricks/rough.png");
        PixelAssert.Equals(45, roughImage);
    }

    [Fact]
    public async Task SupportsTextureSetRenaming()
    {
        await using var graph = Graph();

        await graph.CreateImageAsync("textures/blocks/brick_c.png", 31, 156, 248, 254);
        //await graph.CreateImageAsync("textures/blocks/brick_h.png", 160);
        await graph.CreateImageAsync("textures/blocks/brick_n.png", 127, 127, 200);
        await graph.CreateImageAsync("textures/blocks/brick_s.png", 16, 8, 45);

        var data = new BedrockTextureSet {
            FormatVersion = "1.16.100",
            TextureSet = new JObject {
                ["color"] = "brick_c",
                ["metalness_emissive_roughness"] = "brick_s",
                ["normal"] = "brick_n",
                //["heightmap"] = "brick_h",
            },
        };

        var json = JsonConvert.SerializeObject(data, Formatting.None);
        await graph.CreateFileAsync("textures/blocks/brick.texture_set.json", json);

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

        using var albedoImage = await graph.GetImageAsync<Rgb24>("assets/minecraft/textures/block/bricks/color.png");
        PixelAssert.Equals(31, 156, 248, albedoImage);

        using var opacityImage = await graph.GetImageAsync<L8>("assets/minecraft/textures/block/bricks/opacity.png");
        PixelAssert.Equals(254, opacityImage);

        using var normalImage = await graph.GetImageAsync<Rgb24>("assets/minecraft/textures/block/bricks/normal.png");
        PixelAssert.Equals(127, 127, 255, normalImage);

        //using var heightImage = await graph.GetImageAsync("assets/minecraft/textures/block/bricks/height.png");
        //PixelAssert.RedEquals(160, heightImage);

        using var metalImage = await graph.GetImageAsync<L8>("assets/minecraft/textures/block/bricks/metal.png");
        PixelAssert.Equals(16, metalImage);

        using var emissiveImage = await graph.GetImageAsync<L8>("assets/minecraft/textures/block/bricks/emissive.png");
        PixelAssert.Equals(8, emissiveImage);

        using var roughImage = await graph.GetImageAsync<L8>("assets/minecraft/textures/block/bricks/rough.png");
        PixelAssert.Equals(45, roughImage);
    }
}