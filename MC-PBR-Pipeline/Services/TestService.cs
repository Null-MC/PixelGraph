using McPbrPipeline.Publishing;
using McPbrPipeline.Textures;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Services
{
    internal class TestService : BackgroundService
    {
        private readonly IHostApplicationLifetime lifetime;
        private readonly ITextureLoader loader;
        private readonly ITexturePublisher publisher;


        public TestService(
            IHostApplicationLifetime lifetime,
            ITextureLoader loader,
            ITexturePublisher publisher)
        {
            this.lifetime = lifetime;
            this.loader = loader;
            this.publisher = publisher;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            try {
                await RunAsync(token);
            }
            finally {
                lifetime.StopApplication();
            }
        }

        private async Task RunAsync(CancellationToken token)
        {
            var root = Path.GetFullPath("..\\..\\..\\..\\test-data");

            var profile = new PublishProfile {
                Source = root,
                Destination = Path.Combine(root, "publish"),
                TextureWidth = 128,
                TextureHeight = 128,
            };

            loader.Root = root;
            var filename = Path.Combine(root, "assets\\minecraft\\textures\\block\\pumpkin_stem.json");
            var texture = await loader.LoadTextureAsync(filename, token);

            await publisher.PublishAsync(profile, texture, token);
        }
    }
}
