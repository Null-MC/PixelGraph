using McPbrPipeline.Filters;
using McPbrPipeline.Internal.Filtering;
using Microsoft.Extensions.Hosting;
using SixLabors.ImageSharp;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Services
{
    internal class PrimaryService : BackgroundService
    {
        private readonly IHostApplicationLifetime lifetime;


        public PrimaryService(IHostApplicationLifetime lifetime)
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
            var root = Path.GetFullPath("..\\..\\..\\..");
            var src = Path.Combine(root, "test-data\\assets\\minecraft\\textures\\block\\pumpkin_stem.png");
            var dest = Path.Combine(root, "test-data\\pumpkin_stem_n.png");

            var filters = new FilterCollection();

            filters.Append(new ResizeFilter {
                TargetWidth = 64,
                TargetHeight = 64,
            });

            filters.Append(new NormalMapFilter {
                Options = {
                    Strength = 70f,
                    Wrap = false,
                },
            });

            using var imageSource = await Image.LoadAsync(src, token);

            if (!filters.Empty)
                filters.Apply(imageSource);

            await imageSource.SaveAsPngAsync(dest, token);
        }
    }
}
