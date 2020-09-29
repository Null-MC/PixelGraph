using McPbrPipeline.Publishing;
using McPbrPipeline.Textures;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger logger;


        public TestService(
            ILogger<TestService> logger,
            IHostApplicationLifetime lifetime,
            ITextureLoader loader,
            ITexturePublisher publisher)
        {
            this.lifetime = lifetime;
            this.loader = loader;
            this.publisher = publisher;
            this.logger = logger;
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
                TextureSize = 128,
                TextureHeight = 128,
            };

            await foreach (var texture in loader.LoadAsync(root, token)) {
                await publisher.PublishAsync(profile, texture, token);
            }
        }
    }
}
