using McPbrPipeline.Internal;
using McPbrPipeline.Internal.Textures;
using McPbrPipeline.Tests.Internal;
using Microsoft.Extensions.DependencyInjection;
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

        [InlineData(  0,  0.0f, 255)]
        [InlineData(100,  1.0f,  99)]
        [InlineData(100,  0.5f,  49)]
        [InlineData(100,  2.0f, 199)]
        [InlineData(100,  3.0f, 254)]
        [InlineData(200, 0.01f,   1)]
        [Theory] public async Task Scale(byte value, float scale, byte expected)
        {
            await using var provider = Services.BuildServiceProvider();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            graphBuilder.UseGlobalOutput = true;

            await graphBuilder.BuildAsync(pack, new PbrProperties {
                Name = "test",
                Path = "assets",
                Properties = {
                    ["emissive.value"] = value.ToString(),
                    ["emissive.scale"] = scale.ToString("F"),
                }
            });

            using var image = await Content.OpenImageAsync("assets/test_e.png");
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

            using var emissiveImage = CreateImageR(value);
            await Content.AddAsync("assets/test/emissive.png", emissiveImage);

            await graphBuilder.BuildAsync(pack, new PbrProperties {
                Name = "test",
                Path = "assets",
                Properties = {
                    ["emissive.input.r"] = "emissive-clip",
                }
            });

            using var image = await Content.OpenImageAsync("assets/test_e.png");
            PixelAssert.RedEquals(value, image);
        }

        [InlineData(  0, 255)]
        [InlineData(  1,   0)]
        [InlineData(128, 127)]
        [InlineData(255, 254)]
        [Theory] public async Task ConvertsEmissiveToEmissiveClipped(byte value, byte expected)
        {
            await using var provider = Services.BuildServiceProvider();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            graphBuilder.UseGlobalOutput = true;
            
            using var emissiveImage = CreateImageR(value);
            await Content.AddAsync("assets/test/emissive.png", emissiveImage);

            await graphBuilder.BuildAsync(pack, new PbrProperties {
                Name = "test",
                Path = "assets",
                Properties = {
                    ["emissive.input.r"] = "emissive",
                }
            });

            using var image = await Content.OpenImageAsync("assets/test_e.png");
            PixelAssert.RedEquals(expected, image);
        }
    }
}
