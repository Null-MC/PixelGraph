using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Textures;
using McPbrPipeline.Tests.Internal;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace McPbrPipeline.Tests.ImageTests
{
    public class HeightTests : TestBase
    {
        private readonly PackProperties pack;


        public HeightTests(ITestOutputHelper output) : base(output)
        {
            pack = new PackProperties {
                Properties = {
                    ["output.height.r"] = "height",
                    ["output.height"] = "true",
                }
            };
        }

        [InlineData(  0,  0.0f, 255)]
        [InlineData(100,  1.0f, 155)]
        [InlineData(100,  0.5f, 205)]
        [InlineData(100,  2.0f,  55)]
        [InlineData(100,  3.0f,   0)]
        [InlineData(200, 0.01f, 253)]
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
                    ["height.value"] = value.ToString(),
                    ["height.scale"] = scale.ToString("F"),
                }
            });

            using var image = await Content.OpenImageAsync("assets/test_h.png");
            PixelAssert.RedEquals(expected, image);
        }
    }
}
