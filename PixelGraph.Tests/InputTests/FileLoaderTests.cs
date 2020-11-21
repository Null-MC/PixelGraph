using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.IO;
using PixelGraph.Tests.Internal;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.InputTests
{
    public class FileLoaderTests : TestBase
    {
        public FileLoaderTests(ITestOutputHelper output) : base(output) {}

        [InlineData("assets/junk.zip")]
        [Theory] public async Task IgnoresFiles(string filename)
        {
            Content.Add(filename);

            var items = await LoadFilesAsync();
            Assert.Empty(items);
        }

        [Fact]
        public async Task UntrackedTest()
        {
            Content.Add("assets/1.png");
            Content.Add("assets/2.png");

            var items = await LoadFilesAsync();
            Assert.Equal(2, items.Length);
        }

        [Fact]
        public async Task LocalTextureTest()
        {
            Content.Add("assets/gold_block/pbr.properties", string.Empty);
            Content.Add("assets/gold_block/albedo.png");
            Content.Add("assets/gold_block/normal.png");

            var items = await LoadFilesAsync();
            Assert.Single(items);
        }

        private async Task<object[]> LoadFilesAsync()
        {
            await using var provider = Builder.Build();

            var items = new List<object>();
            var loader = provider.GetRequiredService<IFileLoader>();
            await foreach (var item in loader.LoadAsync()) items.Add(item);
            return items.ToArray();
        }
    }
}
