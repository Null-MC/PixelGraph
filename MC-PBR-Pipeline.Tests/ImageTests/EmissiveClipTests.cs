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
    public class EmissiveClipTests : TestBase
    {
        private readonly PackProperties pack;


        public EmissiveClipTests(ITestOutputHelper output) : base(output)
        {
            pack = new PackProperties {
                Properties = {
                    ["output.emissive.r"] = "emissive-clip",
                    ["output.emissive"] = "true",
                }
            };
        }

        [InlineData(0, 255)]
        [InlineData(1, 0)]
        [InlineData(128, 127)]
        [InlineData(255, 254)]
        [Theory] public async Task ConvertsEmissiveToEmissiveClipped(byte actualEmissive, byte expectedEmissiveClipped)
        {
            var reader = new MockInputReader(Content);
            await using var writer = new MockOutputWriter(Content);
            await using var provider = Services.BuildServiceProvider();
            
            var emissiveColor = new Rgba32(actualEmissive, 0, 0, 0);
            using var emissiveImage = new Image<Rgba32>(Configuration.Default, 1, 1, emissiveColor);
            await Content.AddAsync("assets/test/emissive.png", emissiveImage);

            var graphBuilder = new TextureGraphBuilder(provider, reader, writer, pack) {
                UseGlobalOutput = true,
            };

            await graphBuilder.BuildAsync(new PbrProperties {
                Name = "test",
                Path = "assets",
                Properties = {
                    ["emissive.input.r"] = "emissive",
                }
            });

            using var image = await Content.OpenImageAsync("assets/test_e.png");
            PixelAssert.RedEquals(expectedEmissiveClipped, image);
        }
    }
}
