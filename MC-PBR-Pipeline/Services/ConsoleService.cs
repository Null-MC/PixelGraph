using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Services
{
    internal class ConsoleService : BackgroundService
    {
        private readonly IHostApplicationLifetime lifetime;


        public ConsoleService(IHostApplicationLifetime lifetime)
        {
            this.lifetime = lifetime;
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
            //...
        }
    }
}
