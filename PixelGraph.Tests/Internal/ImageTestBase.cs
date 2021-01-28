using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace PixelGraph.Tests.Internal
{
    public abstract class ImageTestBase : TestBase
    {
        protected ImageTestBase(ITestOutputHelper output) : base(output) {}

        protected ImageTestGraph Graph(MaterialContext context)
        {
            var provider = Builder.Build();
            return new ImageTestGraph(provider, context);
        }
    }

    public class ImageTestGraph : IDisposable, IAsyncDisposable
    {
        private readonly MaterialContext context;
        private readonly ServiceProvider provider;


        public ImageTestGraph(ServiceProvider provider, MaterialContext context)
        {
            this.provider = provider;
            this.context = context;
        }

        public void Dispose()
        {
            provider?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (provider != null)
                await provider.DisposeAsync();
        }

        public Task CreateImageAsync(string localFile, byte gray)
        {
            var content = provider.GetRequiredService<MockFileContent>();

            var color = new L8(gray);
            using var image = new Image<L8>(Configuration.Default, 1, 1, color);
            return content.AddAsync(localFile, image);
        }

        public Task CreateImageAsync(string localFile, byte r, byte g, byte b, byte alpha = 255)
        {
            var content = provider.GetRequiredService<MockFileContent>();

            var color = new Rgba32(r, g, b, alpha);
            using var image = new Image<Rgba32>(Configuration.Default, 1, 1, color);
            return content.AddAsync(localFile, image);
        }

        public async Task ProcessAsync()
        {
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();

            graphBuilder.UseGlobalOutput = true;
            await graphBuilder.ProcessInputGraphAsync(context);
        }

        public Task<Image<Rgba32>> GetImageAsync(string localFile)
        {
            var content = provider.GetRequiredService<MockFileContent>();
            return content.OpenImageAsync(localFile);
        }
    }
}
