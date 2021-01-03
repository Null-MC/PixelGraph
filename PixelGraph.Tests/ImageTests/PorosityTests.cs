using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.Tests.Internal;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.ImageTests
{
    public class PorosityTests : TestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public PorosityTests(ITestOutputHelper output) : base(output)
        {
            packInput = new ResourcePackInputProperties {
                Porosity = {
                    Texture = TextureTags.Porosity,
                    Color = ColorChannel.Red,
                },
            };

            packProfile = new ResourcePackProfileProperties {
                Encoding = {
                    Porosity = {
                        Texture = TextureTags.Porosity,
                        Color = ColorChannel.Red,
                    },
                },
            };
        }

        [InlineData(  0)]
        [InlineData(100)]
        [InlineData(155)]
        [InlineData(255)]
        [Theory] public async Task Passthrough(byte value)
        {
            await using var provider = Builder.Build();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            var content = provider.GetRequiredService<MockFileContent>();
            graphBuilder.UseGlobalOutput = true;

            using var heightImage = CreateImageR(value);
            await content.AddAsync("assets/test/porosity.png", heightImage);

            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            var image = await content.OpenImageAsync("assets/test_p.png");
            PixelAssert.RedEquals(value, image);
        }

        [InlineData(       0, 0.00,   0)]
        [InlineData(       1, 0.00,   0)]
        [InlineData(100/255d, 1.00, 100)]
        [InlineData(100/255d, 0.50,  50)]
        [InlineData(100/255d, 2.00, 200)]
        [InlineData(100/255d, 3.00, 255)]
        [InlineData(200/255d, 0.01,   2)]
        [Theory] public async Task ScaleValue(decimal value, decimal scale, byte expected)
        {
            await using var provider = Builder.Build();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            var content = provider.GetRequiredService<MockFileContent>();
            graphBuilder.UseGlobalOutput = true;

            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                    Porosity = new MaterialPorosityProperties {
                        Value = value,
                        Scale = scale,
                    },
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            var image = await content.OpenImageAsync("assets/test_p.png");
            PixelAssert.RedEquals(expected, image);
        }

        [InlineData(  0, 0.00,   0)]
        [InlineData(100, 1.00, 100)]
        [InlineData(100, 0.50,  50)]
        [InlineData(100, 2.00, 200)]
        [InlineData(100, 3.00, 255)]
        [InlineData(200, 0.01,   2)]
        [Theory] public async Task ScaleTexture(byte value, decimal scale, byte expected)
        {
            await using var provider = Builder.Build();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            var content = provider.GetRequiredService<MockFileContent>();
            graphBuilder.UseGlobalOutput = true;

            using var srcImage = CreateImageR(value);
            await content.AddAsync("assets/test/porosity.png", srcImage);

            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                    Porosity = new MaterialPorosityProperties {
                        Scale = scale,
                    },
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            var image = await content.OpenImageAsync("assets/test_p.png");
            PixelAssert.RedEquals(expected, image);
        }
    }
}
