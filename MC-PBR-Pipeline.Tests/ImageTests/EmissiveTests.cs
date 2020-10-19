using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Textures;
using McPbrPipeline.Tests.Internal;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace McPbrPipeline.Tests.ImageTests
{
    public class EmissiveTests : TestBase
    {
        private readonly PackProperties pack;


        public EmissiveTests(ITestOutputHelper output) : base(output)
        {
            pack = new PackProperties {
                Properties = {
                    ["output.emissive.r"] = "emissive",
                    ["output.emissive"] = "true",
                }
            };
        }

        [InlineData(0, 1)]
        [InlineData(127, 128)]
        [InlineData(254, 255)]
        [InlineData(255, 0)]
        [Theory] public async Task ConvertsEmissiveClippedToEmissive(byte actualEmissiveClipped, byte expectedEmissive)
        {
            var reader = new MockInputReader(Content);
            await using var writer = new MockOutputWriter(Content);
            await using var provider = Services.BuildServiceProvider();
            
            var emissiveColor = new Rgba32(actualEmissiveClipped, 0, 0, 0);
            using var emissiveImage = new Image<Rgba32>(Configuration.Default, 1, 1, emissiveColor);
            await Content.AddAsync("assets/test/emissive.png", emissiveImage);

            var graphBuilder = new TextureGraphBuilder(provider, reader, writer, pack) {
                UseGlobalOutput = true,
            };

            await graphBuilder.BuildAsync(new PbrProperties {
                Name = "test",
                Path = "assets",
                Properties = {
                    ["emissive.input.r"] = "emissive-clip",
                }
            });

            using var image = await Content.OpenImageAsync("assets/test_e.png");
            PixelAssert.RedEquals(expectedEmissive, image);
        }
    }
}
