using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Tests.Internal;
using PixelGraph.Tests.Internal.Mocks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.InputTests;

public class FileLoaderTests : TestBase
{
    public FileLoaderTests(ITestOutputHelper output) : base(output)
    {
        Builder.ConfigureReader(ContentTypes.File, GameEditions.None, null);
    }

    [InlineData("assets/junk.zip")]
    [Theory] public async Task IgnoresFiles(string filename)
    {
        await using var provider = Builder.Build();
        var content = provider.GetRequiredService<MockFileContent>();
        content.Add(filename);

        var items = await LoadFilesAsync(provider);
        Assert.Empty(items);
    }

    [Fact]
    public async Task UntrackedTest()
    {
        await using var provider = Builder.Build();
        var content = provider.GetRequiredService<MockFileContent>();
        content.Add("assets/1.png");
        content.Add("assets/2.png");

        var items = await LoadFilesAsync(provider);
        Assert.Equal(2, items.Length);
    }

    [Fact]
    public async Task LocalTextureTest()
    {
        await using var provider = Builder.Build();
        var content = provider.GetRequiredService<MockFileContent>();
        await content.AddAsync("assets/gold_block/pbr.yml", string.Empty);
        content.Add("assets/gold_block/albedo.png");
        content.Add("assets/gold_block/normal.png");

        var items = await LoadFilesAsync(provider);
        Assert.Single(items);
    }

    private async Task<object[]> LoadFilesAsync(IServiceProvider provider)
    {
        //await using var provider = Builder.Build();

        var items = new List<object>();
        var loader = provider.GetRequiredService<IPublishReader>();
        await foreach (var item in loader.LoadAsync()) items.Add(item);
        return items.ToArray();
    }
}