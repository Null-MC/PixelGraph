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

public class BedrockMaterialReaderTests : TestBase, IDisposable, IAsyncDisposable
{
    private readonly ServiceProvider provider;


    public BedrockMaterialReaderTests(ITestOutputHelper output) : base(output)
    {
        Builder.ConfigureReader(ContentTypes.File, GameEditions.Bedrock, null);

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

    [InlineData("~/basecolor.png", TextureTags.Color)]
    [InlineData("~/heightmap.png", TextureTags.Height)]
    [InlineData("~/normal.png", TextureTags.Normal)]
    [InlineData("~/mer.png", TextureTags.MER)]
    [Theory] public void ReadsLocalFile(string filename, string type)
    {
        var reader = provider.GetRequiredService<ITextureReader>();
        Assert.True(reader.IsLocalFile(filename, type));
    }

    [InlineData("~/test.png", TextureTags.Color)]
    [InlineData("~/test_heightmap.png", TextureTags.Height)]
    [InlineData("~/test_normal.png", TextureTags.Normal)]
    [InlineData("~/test_mer.png", TextureTags.MER)]
    [Theory] public void ReadsGlobalType(string filename, string type)
    {
        var reader = provider.GetRequiredService<ITextureReader>();
        Assert.True(reader.IsGlobalFile(filename, "test", type));
    }
}