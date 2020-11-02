using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.Textures;
using PixelGraph.Tests.Internal;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.ImageTests
{
    public class AlbedoTests : TestBase
    {
        private readonly PackProperties pack;


        public AlbedoTests(ITestOutputHelper output) : base(output)
        {
            pack = new PackProperties {
                Properties = {
                    ["output.albedo.r"] = "albedo-r",
                    ["output.albedo.g"] = "albedo-g",
                    ["output.albedo.b"] = "albedo-b",
                    ["output.albedo.a"] = "albedo-a",
                    ["output.albedo"] = "true",
                }
            };
        }

        [InlineData(  0,  0.0f,   0)]
        [InlineData(100,  1.0f, 100)]
        [InlineData(100,  0.5f,  50)]
        [InlineData(100,  2.0f, 200)]
        [InlineData(100,  3.0f, 255)]
        [InlineData(200, 0.01f,   2)]
        [Theory] public async Task ScaleRed(byte value, float scale, byte expected)
        {
            await using var provider = Builder.Build();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            graphBuilder.UseGlobalOutput = true;

            await graphBuilder.BuildAsync(pack, new PbrProperties {
                Name = "test",
                Path = "assets",
                Properties = {
                    ["albedo.value.r"] = value.ToString(),
                    ["albedo.scale.r"] = scale.ToString("F"),
                }
            });

            using var image = await Content.OpenImageAsync("assets/test.png");
            PixelAssert.RedEquals(expected, image);
        }

        [Fact]
        public async Task ShiftRGB()
        {
            await using var provider = Builder.Build();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            graphBuilder.UseGlobalOutput = true;

            using var image = CreateImage(60, 120, 180);
            await Content.AddAsync("assets/test/albedo.png", image);

            await graphBuilder.BuildAsync(pack, new PbrProperties {
                Name = "test",
                Path = "assets",
                Properties = {
                    ["albedo.input.r"] = "albedo-b",
                    ["albedo.input.g"] = "albedo-r",
                    ["albedo.input.b"] = "albedo-g",
                }
            });

            using var outputImage = await Content.OpenImageAsync("assets/test.png");
            PixelAssert.RedEquals(120, outputImage);
            PixelAssert.GreenEquals(180, outputImage);
            PixelAssert.BlueEquals(60, outputImage);
        }
    }
}
