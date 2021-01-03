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
    public class HeightTests : TestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public HeightTests(ITestOutputHelper output) : base(output)
        {
            packInput = new ResourcePackInputProperties {
                Height = {
                    Texture = TextureTags.Height,
                    Color = ColorChannel.Red,
                    Invert = true,
                },
            };

            packProfile = new ResourcePackProfileProperties {
                Encoding = {
                    Height = {
                        Texture = TextureTags.Height,
                        Color = ColorChannel.Red,
                        Invert = true,
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
            await content.AddAsync("assets/test/height.png", heightImage);
            
            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            var image = await content.OpenImageAsync("assets/test_h.png");
            PixelAssert.RedEquals(value, image);
        }

        [InlineData(0.000, 0.00, 255)]
        [InlineData(1.000, 0.00, 255)]
        [InlineData(0.392, 1.00, 155)]
        [InlineData(0.392, 0.50, 205)]
        [InlineData(0.392, 2.00,  55)]
        [InlineData(0.392, 3.00,   0)]
        [InlineData(0.784, 0.01, 253)]
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
                    Height = new MaterialHeightProperties {
                        Value = value,
                        Scale = scale,
                    },
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            var image = await content.OpenImageAsync("assets/test_h.png");
            PixelAssert.RedEquals(expected, image);
        }

        [InlineData(  0, 0.00, 255)]
        [InlineData(255, 0.00, 255)]
        [InlineData(100, 1.00, 100)]
        [InlineData(155, 0.50, 205)]
        [InlineData(155, 2.00,  55)]
        [InlineData(155, 3.00,   0)]
        [InlineData( 55, 0.01, 253)]
        [Theory] public async Task ScaleTexture(byte value, decimal scale, byte expected)
        {
            await using var provider = Builder.Build();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            var content = provider.GetRequiredService<MockFileContent>();
            graphBuilder.UseGlobalOutput = true;

            using var heightImage = CreateImageR(value);
            await content.AddAsync("assets/test/height.png", heightImage);

            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                    Height = new MaterialHeightProperties {
                        Scale = scale,
                    },
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            var image = await content.OpenImageAsync("assets/test_h.png");
            PixelAssert.RedEquals(expected, image);
        }
    }
}
