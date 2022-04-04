using Microsoft.Extensions.DependencyInjection;
using PixelGraph.CLI.CommandLine;
using PixelGraph.Common.IO;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Tests.Internal;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.CommandLineTests
{
    public class PublishTests : TestBase, IAsyncDisposable, IDisposable
    {
        private readonly ServiceProvider provider;


        public PublishTests(ITestOutputHelper output) : base(output)
        {
            Builder.Services.AddScoped<PublishCommand.Executor>();

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

        [Fact]
        public async Task CanPublishToDirectoryTest()
        {
            var executor = provider.GetRequiredService<PublishCommand.Executor>();

            executor.Context = new ResourcePackContext {
                Input = new ResourcePackInputProperties {
                    Edition = null,
                    Format = TextureFormat.Format_Raw,
                    //...
                },
                Profile = new ResourcePackProfileProperties {
                    Edition = GameEdition.Java,
                    Encoding = {
                        Format = TextureFormat.Format_Lab13,
                    }
                }
            };

            await executor.ExecuteAsync("sourceDir", "destDir");

            //...
        }

        [Fact]
        public async Task CanPublishToArchiveTest()
        {
            var executor = provider.GetRequiredService<PublishCommand.Executor>();
            executor.AsArchive = true;

            executor.Context = new ResourcePackContext {
                Input = new ResourcePackInputProperties {
                    Edition = null,
                    Format = TextureFormat.Format_Raw,
                    //...
                },
                Profile = new ResourcePackProfileProperties {
                    Edition = GameEdition.Java,
                    Encoding = {
                        Format = TextureFormat.Format_Lab13,
                    }
                }
            };

            await executor.ExecuteAsync("sourceDir", "destDir");

            //...
        }
    }
}
