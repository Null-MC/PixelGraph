using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.Textures;
using PixelGraph.Tests.Internal;
using System.Threading.Tasks;
using PixelGraph.Common.Encoding;
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
                    ["output.albedo.r"] = EncodingChannel.Red,
                    ["output.albedo.g"] = EncodingChannel.Green,
                    ["output.albedo.b"] = EncodingChannel.Blue,
                    ["output.albedo.a"] = EncodingChannel.Alpha,
                    ["output.albedo"] = bool.TrueString,
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
                    ["albedo.input.r"] = EncodingChannel.Blue,
                    ["albedo.input.g"] = EncodingChannel.Red,
                    ["albedo.input.b"] = EncodingChannel.Green,
                }
            });

            using var outputImage = await Content.OpenImageAsync("assets/test.png");
            PixelAssert.RedEquals(120, outputImage);
            PixelAssert.GreenEquals(180, outputImage);
            PixelAssert.BlueEquals(60, outputImage);
        }
    }
}
