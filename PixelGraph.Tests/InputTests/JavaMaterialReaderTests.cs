using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.IO.Texture;
using PixelGraph.Common.Textures;
using PixelGraph.Tests.Internal;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.InputTests
{
    public class JavaMaterialReaderTests : TestBase, IDisposable, IAsyncDisposable
    {
        private readonly ServiceProvider provider;


        public JavaMaterialReaderTests(ITestOutputHelper output) : base(output)
        {
            Builder.AddContentReader(ContentTypes.File);
            Builder.AddTextureReader(GameEditions.Java);

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
        [InlineData("~/normal.png", TextureTags.Normal)]
        [InlineData("~/specular.png", TextureTags.Specular)]
        [Theory] public void ReadsLocalFile(string filename, string type)
        {
            var reader = provider.GetRequiredService<ITextureReader>();
            Assert.True(reader.IsLocalFile(filename, type));
        }

        [InlineData("~/test.png", TextureTags.Color)]
        [InlineData("~/test_n.png", TextureTags.Normal)]
        [InlineData("~/test_s.png", TextureTags.Specular)]
        [Theory] public void ReadsGlobalType(string filename, string type)
        {
            var reader = provider.GetRequiredService<ITextureReader>();
            Assert.True(reader.IsGlobalFile(filename, "test", type));
        }
    }
}
