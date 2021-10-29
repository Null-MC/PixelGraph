using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.Common.Textures.Graphing;
using PixelGraph.Tests.Internal.Mocks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PixelGraph.Common.Textures.Graphing.Builders;
using Xunit.Abstractions;

namespace PixelGraph.Tests.Internal
{
    public abstract class ImageTestBase : TestBase
    {
        protected ImageTestBase(ITestOutputHelper output) : base(output) {}

        protected ImageTestGraph Graph()
        {
            var provider = Builder.Build();
            return new ImageTestGraph(provider);
        }
    }

    public class ImageTestGraph : IDisposable, IAsyncDisposable
    {
        private readonly ServiceProvider provider;

        public IServiceProvider Provider => provider;
        public ResourcePackInputProperties PackInput {get; set;}
        public ResourcePackProfileProperties PackProfile {get; set;}
        public MaterialProperties Material {get; set;}


        public ImageTestGraph(ServiceProvider provider)
        {
            this.provider = provider;
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

        public Task CreateImageAsync(string localFile, byte gray)
        {
            var content = provider.GetRequiredService<MockFileContent>();

            var color = new L8(gray);
            using var image = new Image<L8>(Configuration.Default, 1, 1, color);
            return content.AddAsync(localFile, image);
        }

        public Task CreateImageAsync(string localFile, byte r, byte g, byte b, byte alpha = 255)
        {
            //return CreateImageAsync(localFile, 1, 1, r, g, b, alpha);
            var content = provider.GetRequiredService<MockFileContent>();

            var color = new Rgba32(r, g, b, alpha);
            using var image = new Image<Rgba32>(Configuration.Default, 1, 1, color);
            return content.AddAsync(localFile, image);
        }

        //public Task CreateImageAsync(string localFile, int width, int height)
        //{
        //    var content = provider.GetRequiredService<MockFileContent>();

        //    var color = new Rgba32(r, g, b, alpha);
        //    using var image = new Image<Rgba32>(Configuration.Default, width, height, color);
        //    return content.AddAsync(localFile, image);
        //}

        public Task CreateImageAsync<TPixel>(string localFile, int width, int height, Action<Image<TPixel>> initAction)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var content = provider.GetRequiredService<MockFileContent>();

            using var image = new Image<TPixel>(Configuration.Default, width, height);

            initAction(image);

            return content.AddAsync(localFile, image);
        }

        public async Task ProcessAsync(CancellationToken token = default)
        {
            using var scope = provider.CreateScope();
            var graphContext = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();
            var graphBuilder = scope.ServiceProvider.GetRequiredService<IPublishGraphBuilder>();

            graphContext.Input = PackInput;
            graphContext.Profile = PackProfile;
            graphContext.Material = Material;
            //graphContext.UseGlobalOutput = true;
            graphContext.Mapping = new DefaultPublishMapping();

            await graphBuilder.PublishAsync(token);
            await graphBuilder.PublishInventoryAsync("inventory", token);
        }

        public Task<Image<Rgba32>> GetImageAsync(string localFile)
        {
            var content = provider.GetRequiredService<MockFileContent>();
            return content.OpenImageAsync(localFile);
        }

        public Stream GetFile(string localFile)
        {
            var content = provider.GetRequiredService<MockFileContent>();
            return content.OpenRead(localFile);
        }
    }
}
