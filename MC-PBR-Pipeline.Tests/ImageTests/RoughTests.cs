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
    public class RoughTests : TestBase
    {
        private readonly PackProperties pack;


        public RoughTests(ITestOutputHelper output) : base(output)
        {
            pack = new PackProperties {
                Properties = {
                    ["output.rough.r"] = "rough",
                    ["output.rough"] = "true",
                }
            };
        }

        [InlineData(0)]
        [InlineData(100)]
        [InlineData(155)]
        [InlineData(255)]
        [Theory] public async Task Passthrough(byte value)
        {
            var reader = new MockInputReader(Content);
            await using var writer = new MockOutputWriter(Content);
            await using var provider = Services.BuildServiceProvider();

            var roughColor = new Rgba32(value, 0, 0, 0);
            using var roughImage = new Image<Rgba32>(Configuration.Default, 1, 1, roughColor);
            await Content.AddAsync("assets/test/rough.png", roughImage);

            var graphBuilder = new TextureGraphBuilder(provider, reader, writer, pack) {
                UseGlobalOutput = true,
            };

            await graphBuilder.BuildAsync(new PbrProperties {
                Name = "test",
                Path = "assets",
                Properties = {
                    ["rough.input.r"] = "rough",
                }
            });

            using var image = await Content.OpenImageAsync("assets/test_rough.png");
            PixelAssert.RedEquals(value, image);
        }

        [InlineData(0, 255)]
        [InlineData(100, 155)]
        [InlineData(155, 100)]
        [InlineData(255, 0)]
        [Theory] public async Task ConvertsSmoothToRough(byte actualSmooth, byte expectedRough)
        {
            var reader = new MockInputReader(Content);
            await using var writer = new MockOutputWriter(Content);
            await using var provider = Services.BuildServiceProvider();

            var smoothColor = new Rgba32(actualSmooth, 0, 0, 0);
            using var smoothImage = new Image<Rgba32>(Configuration.Default, 1, 1, smoothColor);
            await Content.AddAsync("assets/test/smooth.png", smoothImage);

            var graphBuilder = new TextureGraphBuilder(provider, reader, writer, pack) {
                UseGlobalOutput = true,
            };

            await graphBuilder.BuildAsync(new PbrProperties {
                Name = "test",
                Path = "assets",
                Properties = {
                    ["smooth.input.r"] = "smooth",
                }
            });

            using var image = await Content.OpenImageAsync("assets/test_rough.png");
            PixelAssert.RedEquals(expectedRough, image);
        }
    }
}
