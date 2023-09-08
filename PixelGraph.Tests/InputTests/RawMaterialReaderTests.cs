using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.IO.Texture;
using PixelGraph.Common.Textures;
using PixelGraph.Tests.Internal;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.InputTests;

public class RawMaterialReaderTests : TestBase, IDisposable, IAsyncDisposable
{
    private readonly ServiceProvider provider;


    public RawMaterialReaderTests(ITestOutputHelper output) : base(output)
    {
        Builder.ConfigureReader(ContentTypes.File, GameEditions.None, null);

        provider = Builder.Build();
    }

    public void Dispose()
    {
        provider?.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (provider != null)
            await provider.DisposeAsync();
    }

    // Substance
    [InlineData("~/Substance_graph_basecolor.png", TextureTags.Color)]
    [InlineData("~/Substance_graph_ambientocclusion.png", TextureTags.Occlusion)]
    [InlineData("~/Substance_graph_height.png", TextureTags.Height)]
    [InlineData("~/Substance_graph_metal.png", TextureTags.Metal)]
    [InlineData("~/Substance_graph_metallic.png", TextureTags.Metal)]
    [InlineData("~/Substance_graph_normal.png", TextureTags.Normal)]
    [InlineData("~/Substance_graph_porosity.png", TextureTags.Porosity)]
    [InlineData("~/Substance_graph_roughness.png", TextureTags.Rough)]
    [InlineData("~/Substance_graph_SSS.png", TextureTags.SubSurfaceScattering)]
    [InlineData("~/Substance_graph_emissive.png", TextureTags.Emissive)]

    [InlineData("~/opacity.png", TextureTags.Opacity)]
    [InlineData("~/alpha.png", TextureTags.Opacity)]
    [InlineData("~/color.png", TextureTags.Color)]
    [InlineData("~/basecolor.png", TextureTags.Color)]
    [InlineData("~/albedo.png", TextureTags.Color)]
    [InlineData("~/diffuse.png", TextureTags.Color)]
    [InlineData("~/height.png", TextureTags.Height)]
    [InlineData("~/bump.png", TextureTags.Bump)]
    [InlineData("~/normal.png", TextureTags.Normal)]
    [InlineData("~/occlusion.png", TextureTags.Occlusion)]
    [InlineData("~/ambientOcclusion.png", TextureTags.Occlusion)]
    [InlineData("~/AO.png", TextureTags.Occlusion)]
    [InlineData("~/specular.png", TextureTags.Specular)]
    [InlineData("~/smooth.png", TextureTags.Smooth)]
    [InlineData("~/smoothness.png", TextureTags.Smooth)]
    [InlineData("~/rough.png", TextureTags.Rough)]
    [InlineData("~/roughness.png", TextureTags.Rough)]
    [InlineData("~/f0.png", TextureTags.F0)]
    [InlineData("~/metal.png", TextureTags.Metal)]
    [InlineData("~/metallic.png", TextureTags.Metal)]
    [InlineData("~/metalness.png", TextureTags.Metal)]
    [InlineData("~/hcm.png", TextureTags.HCM)]
    [InlineData("~/emissive.png", TextureTags.Emissive)]
    [InlineData("~/emission.png", TextureTags.Emissive)]
    [InlineData("~/porosity.png", TextureTags.Porosity)]
    [InlineData("~/sss.png", TextureTags.SubSurfaceScattering)]
    [InlineData("~/scattering.png", TextureTags.SubSurfaceScattering)]
    [Theory] public void ReadsLocalFile(string filename, string type)
    {
        var reader = provider.GetRequiredService<ITextureReader>();
        Assert.True(reader.IsLocalFile(filename, type));
    }

    [InlineData("~/test_opacity.png", TextureTags.Opacity)]
    [InlineData("~/test_color.png", TextureTags.Color)]
    [InlineData("~/test_height.png", TextureTags.Height)]
    [InlineData("~/test_bump.png", TextureTags.Bump)]
    [InlineData("~/test_normal.png", TextureTags.Normal)]
    [InlineData("~/test_occlusion.png", TextureTags.Occlusion)]
    [InlineData("~/test_specular.png", TextureTags.Specular)]
    [InlineData("~/test_smooth.png", TextureTags.Smooth)]
    [InlineData("~/test_rough.png", TextureTags.Rough)]
    [InlineData("~/test_f0.png", TextureTags.F0)]
    [InlineData("~/test_metal.png", TextureTags.Metal)]
    [InlineData("~/test_hcm.png", TextureTags.HCM)]
    [InlineData("~/test_emissive.png", TextureTags.Emissive)]
    [InlineData("~/test_porosity.png", TextureTags.Porosity)]
    [InlineData("~/test_sss.png", TextureTags.SubSurfaceScattering)]
    [Theory] public void ReadsGlobalType(string filename, string type)
    {
        var reader = provider.GetRequiredService<ITextureReader>();
        Assert.True(reader.IsGlobalFile(filename, "test", type));
    }
}