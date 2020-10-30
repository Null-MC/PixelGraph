using McPbrPipeline.Internal;
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
            await using var provider = Services.BuildServiceProvider();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            graphBuilder.UseGlobalOutput = true;

            await graphBuilder.BuildAsync(pack, new PbrProperties {
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

        [InlineData(  0)]
        [InlineData(100)]
        [InlineData(155)]
        [InlineData(255)]
        [Theory] public async Task Passthrough(byte value)
        {
            await using var provider = Services.BuildServiceProvider();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            graphBuilder.UseGlobalOutput = true;

            using var heightImage = CreateImageR(value);
            await Content.AddAsync("assets/test/height.png", heightImage);

            await graphBuilder.BuildAsync(pack, new PbrProperties {
                Name = "test",
                Path = "assets",
                Properties = {
                    ["height.input.r"] = "height",
                }
            });

            using var image = await Content.OpenImageAsync("assets/test_h.png");
            PixelAssert.RedEquals(value, image);
        }
    }
}
