using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Textures;
using McPbrPipeline.Tests.Internal;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace McPbrPipeline.Tests.ImageTests
{
    public class PorositySSSTests : TestBase
    {
        private readonly PackProperties pack;


        public PorositySSSTests(ITestOutputHelper output) : base(output)
        {
            pack = new PackProperties {
                Properties = {
                    ["output.specular.r"] = "porosity-sss",
                    ["output.specular"] = "true",
                }
            };
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
            await Content.AddAsync("assets/test/specular.png", heightImage);

            var graphBuilder = new TextureGraphBuilder(provider, reader, writer, pack) {
                UseGlobalOutput = true,
            };

            await graphBuilder.BuildAsync(new PbrProperties {
                Name = "test",
                Path = "assets",
                Properties = {
                    ["specular.input.r"] = "porosity-sss",
                }
            });

            using var image = await Content.OpenImageAsync("assets/test_s.png");
            PixelAssert.RedEquals(value, image);
        }

        [InlineData(  0,  0)]
        [InlineData(100, 25)]
        [InlineData(200, 50)]
        [InlineData(255, 64)]
        [Theory] public async Task ConvertsPorosityToPSSS(byte value, byte expected)
        {
            var reader = new MockInputReader(Content);
            await using var writer = new MockOutputWriter(Content);
            await using var provider = Services.BuildServiceProvider();

            using var porosityImage = CreateImageR(value);
            await Content.AddAsync("assets/test/porosity.png", porosityImage);

            var graphBuilder = new TextureGraphBuilder(provider, reader, writer, pack) {
                UseGlobalOutput = true,
            };

            await graphBuilder.BuildAsync(new PbrProperties {
                Name = "test",
                Path = "assets",
                Properties = {
                    ["porosity.input.r"] = "porosity",
                }
            });

            using var image = await Content.OpenImageAsync("assets/test_s.png");
            PixelAssert.RedEquals(expected, image);
        }

        [InlineData(  0,  65)]
        [InlineData(100, 140)]
        [InlineData(200, 214)]
        [InlineData(253, 254)]
        [InlineData(255, 255)]
        [Theory] public async Task ConvertsSSSToPSSS(byte value, byte expected)
        {
            var reader = new MockInputReader(Content);
            await using var writer = new MockOutputWriter(Content);
            await using var provider = Services.BuildServiceProvider();

            using var porosityImage = CreateImageR(value);
            await Content.AddAsync("assets/test/sss.png", porosityImage);

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

            using var image = await Content.OpenImageAsync("assets/test_s.png");
            PixelAssert.RedEquals(expected, image);
        }
    }
}
