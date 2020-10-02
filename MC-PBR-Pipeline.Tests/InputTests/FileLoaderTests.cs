using McPbrPipeline.Internal.Input;
using McPbrPipeline.Tests.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace McPbrPipeline.Tests.InputTests
{
    public class FileLoaderTests
    {
        private readonly ServiceCollection services;


        public FileLoaderTests(ITestOutputHelper output)
        {
            services = new ServiceCollection();
            services.AddSingleton(output);
            services.AddSingleton(typeof(ILogger<>), typeof(TestLogger<>));
            services.AddSingleton<ILogger, TestLogger>();
        }

        [Fact]
        public async Task IgnoreTest()
        {
            var reader = new MockInputReader {
                Root = "root",
                Content = {
                    Files = {
                        Path.Combine("root", "pack.json"),
                        Path.Combine("root", "assets", "junk.zip"),
                    },
                },
            };

            var items = await LoadFilesAsync(reader);

            Assert.Empty(items);
        }

        [Fact]
        public async Task UntrackedTest()
        {
            var reader = new MockInputReader {
                Root = "root",
                Content = {
                    Files = {
                        Path.Combine("root", "assets", "1.png"),
                        Path.Combine("root", "assets", "2.png"),
                    },
                },
            };

            var items = await LoadFilesAsync(reader);
            Assert.Equal(2, items.Length);
        }

        [Fact]
        public async Task LocalTextureTest()
        {
            var reader = new MockInputReader {
                Root = "root",
                Content = {
                    Files = {
                        Path.Combine("root", "assets", "gold_block", "pbr.json"),
                        Path.Combine("root", "assets", "gold_block", "albedo.png"),
                        Path.Combine("root", "assets", "gold_block", "normal.png"),
                    },
                },
            };

            var items = await LoadFilesAsync(reader);
            Assert.Single(items);
        }

        private async Task<object[]> LoadFilesAsync(IInputReader reader)
        {
            await using var provider = services.BuildServiceProvider();
            var loader = new FileLoader(provider, reader);

            var items = new List<object>();
            await foreach (var item in loader.LoadAsync()) items.Add(item);
            return items.ToArray();
        }
    }
}
