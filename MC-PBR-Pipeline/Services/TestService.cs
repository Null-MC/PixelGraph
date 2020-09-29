using McPbrPipeline.Internal.Publishing;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Services
{
    internal class TestService : BackgroundService
    {
        private readonly IHostApplicationLifetime lifetime;
        private readonly IPublisher publisher;


        public TestService(
            IHostApplicationLifetime lifetime,
            IPublisher publisher)
        {
            this.lifetime = lifetime;
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
            const string source = @"D:\MC-NULL-PBR";
            const string destination = @"C:\Users\null5\AppData\Roaming\.minecraft\profiles\vanilla_1.16.2\resourcepacks\NULL-PBR-128x";

            var timer = Stopwatch.StartNew();
            await publisher.PublishAsync(source, destination, token);
            timer.Stop();

            Log.Information($"Duration: {timer.Elapsed:g}");
        }
    }
}
