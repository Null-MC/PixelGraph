using McPbrPipeline.Internal.Input;
using McPbrPipeline.Tests.Internal;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace McPbrPipeline.Tests.InputTests
{
    public class FileLoaderTests : TestBase
    {
        private readonly MockInputReader reader;


        public FileLoaderTests(ITestOutputHelper output) : base(output)
        {
            reader = new MockInputReader(Content);
        }

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
            await Content.AddAsync("assets/gold_block/pbr.properties", string.Empty);
            Content.Add("assets/gold_block/albedo.png");
            Content.Add("assets/gold_block/normal.png");

            var items = await LoadFilesAsync();
            Assert.Single(items);
        }

        private async Task<object[]> LoadFilesAsync()
        {
            await using var provider = Services.BuildServiceProvider();
            var loader = new FileLoader(provider, reader);

            var items = new List<object>();
            await foreach (var item in loader.LoadAsync()) items.Add(item);
            return items.ToArray();
        }
    }
}
