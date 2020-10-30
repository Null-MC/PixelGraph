using McPbrPipeline.Internal;
using McPbrPipeline.Internal.Textures;
using McPbrPipeline.Tests.Internal;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace McPbrPipeline.Tests.ImageTests
{
    public class PerceptualSmoothTests : TestBase
    {
        private readonly PackProperties pack;


        public PerceptualSmoothTests(ITestOutputHelper output) : base(output)
        {
            pack = new PackProperties {
                Properties = {
                    ["output.smooth.r"] = "smooth2",
                    ["output.smooth"] = "true",
                }
            };
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

            using var smoothImage = CreateImageR(value);
            await Content.AddAsync("assets/test/smooth.png", smoothImage);

            await graphBuilder.BuildAsync(pack, new PbrProperties {
                Name = "test",
                Path = "assets",
                Properties = {
                    ["smooth.input.r"] = "smooth2",
                }
            });

            using var image = await Content.OpenImageAsync("assets/test_smooth.png");
            PixelAssert.RedEquals(value, image);
        }

        [InlineData(  0,   0)]
        [InlineData(100, 160)]
        [InlineData(200, 226)]
        [InlineData(255, 255)]
        [Theory] public async Task ConvertsSmoothToPerceptualSmooth(byte value, byte expected)
        {
            await using var provider = Services.BuildServiceProvider();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            graphBuilder.UseGlobalOutput = true;

            using var smoothImage = CreateImageR(value);
            await Content.AddAsync("assets/test/smooth.png", smoothImage);

            await graphBuilder.BuildAsync(pack, new PbrProperties {
                Name = "test",
                Path = "assets",
                Properties = {
                    ["smooth.input.r"] = "smooth",
                }
            });

            using var image = await Content.OpenImageAsync("assets/test_smooth.png");
            PixelAssert.RedEquals(expected, image);
        }
    }
}
