using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Textures;
using McPbrPipeline.Tests.Internal;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace McPbrPipeline.Tests.ImageTests
{
    public class SubSurfaceScatteringTests : TestBase
    {
        private readonly PackProperties pack;


        public SubSurfaceScatteringTests(ITestOutputHelper output) : base(output)
        {
            pack = new PackProperties {
                Properties = {
                    ["output.sss.r"] = "sss",
                    ["output.sss"] = "true",
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
                    ["sss.value"] = value.ToString(),
                    ["sss.scale"] = scale.ToString("F"),
                }
            });

            using var image = await Content.OpenImageAsync("assets/test_sss.png");
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

            using var heightImage = CreateImageR(value);
            await Content.AddAsync("assets/test/sss.png", heightImage);

            var graphBuilder = new TextureGraphBuilder(provider, reader, writer, pack) {
                UseGlobalOutput = true,
            };

            await graphBuilder.BuildAsync(new PbrProperties {
                Name = "test",
                Path = "assets",
                Properties = {
                    ["sss.input.r"] = "sss",
                }
            });

            using var image = await Content.OpenImageAsync("assets/test_sss.png");
            PixelAssert.RedEquals(value, image);
        }
    }
}
