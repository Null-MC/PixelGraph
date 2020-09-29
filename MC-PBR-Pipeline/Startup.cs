using McPbrPipeline.Internal.Publishing;
using McPbrPipeline.Internal.Textures;
using McPbrPipeline.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace McPbrPipeline
{
    internal class Startup
    {
        public IConfiguration Configuration {get;}


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IPublisher, Publisher>();
            services.AddSingleton<ITextureLoader, TextureLoader>();
            services.AddSingleton<ITexturePublisher, TexturePublisher>();

            services.AddHostedService<TestService>();
        }
    }
}
