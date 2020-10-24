using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Textures;
using McPbrPipeline.Tests.Internal;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace McPbrPipeline.Tests.ImageTests
{
    public class SmoothTests : TestBase
    {
        private readonly PackProperties pack;


        public SmoothTests(ITestOutputHelper output) : base(output)
        {
            pack = new PackProperties {
                Properties = {
                    ["output.smooth.r"] = "smooth",
                    ["output.smooth"] = "true",
                }
            };
        }

        [InlineData(  0,  0.0f,   0)]
        [InlineData(100,  1.0f, 100)]
        [InlineData(100,  0.5f,  50)]
        [InlineData(100,  2.0f, 200)]
        [InlineData(100,  3.0f, 255)]
        [InlineData(200, 0.01f,   2)]
        [Theory] public async Task Scale(byte value, float scale, byte expected)
        {
            var reader = new MockInputReader(Content);
            await using var writer = new MockOutputWriter(Content);
            await using var provider = Services.BuildServiceProvider();

            var graphBuilder = new TextureGraphBuilder(provider, reader, writer, pack) {
                UseGlobalOutput = true,
            };

            await graphBuilder.BuildAsync(new PbrProperties {
                Name = "test",
                Path = "assets",
                Properties = {
                    ["smooth.value"] = value.ToString(),
                    ["smooth.scale"] = scale.ToString("F"),
                }
            });

            using var image = await Content.OpenImageAsync("assets/test_smooth.png");
            PixelAssert.RedEquals(expected, image);
        }

        [InlineData(  0)]
        [InlineData(100)]
        [InlineData(155)]
        [InlineData(255)]
        [Theory] public async Task Passthrough(byte value)
        {
            var reader = new MockInputReader(Content);
            await using var writer = new MockOutputWriter(Content);
            await using var provider = Services.BuildServiceProvider();

            using var smoothImage = CreateImageR(value);
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

            using var image = await Content.OpenImageAsync("assets/test_smooth.png");
            PixelAssert.RedEquals(value, image);
        }

        [InlineData(  0, 255)]
        [InlineData(100, 155)]
        [InlineData(155, 100)]
        [InlineData(255,   0)]
        [Theory] public async Task ConvertsRoughToSmooth(byte value, byte expected)
        {
            var reader = new MockInputReader(Content);
            await using var writer = new MockOutputWriter(Content);
            await using var provider = Services.BuildServiceProvider();

            using var roughImage = CreateImageR(value);
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

            using var image = await Content.OpenImageAsync("assets/test_smooth.png");
            PixelAssert.RedEquals(expected, image);
        }

        [InlineData(  0,   0)]
        [InlineData(160, 100)]
        [InlineData(226, 200)]
        [InlineData(255, 255)]
        [Theory] public async Task ConvertsPerceptualSmoothToSmooth(byte value, byte expected)
        {
            var reader = new MockInputReader(Content);
            await using var writer = new MockOutputWriter(Content);
            await using var provider = Services.BuildServiceProvider();

            using var smoothImage = CreateImageR(value);
            await Content.AddAsync("assets/test/smooth.png", smoothImage);

            var graphBuilder = new TextureGraphBuilder(provider, reader, writer, pack) {
                UseGlobalOutput = true,
            };

            await graphBuilder.BuildAsync(new PbrProperties {
                Name = "test",
                Path = "assets",
                Properties = {
                    ["smooth.input.r"] = "smooth2",
                }
            });

            using var image = await Content.OpenImageAsync("assets/test_smooth.png");
            PixelAssert.RedEquals(expected, image);
        }
    }
}
