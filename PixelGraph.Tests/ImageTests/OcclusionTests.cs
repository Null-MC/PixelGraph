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
    public class OcclusionTests : TestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public OcclusionTests(ITestOutputHelper output) : base(output)
        {
            packInput = new ResourcePackInputProperties {
                Occlusion = {
                    Texture = TextureTags.Occlusion,
                    Color = ColorChannel.Red,
                    Invert = true,
                },
            };

            packProfile = new ResourcePackProfileProperties {
                Encoding = {
                    Occlusion = {
                        Texture = TextureTags.Occlusion,
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

            using var sourceImage = CreateImageR(value);
            await content.AddAsync("assets/test/occlusion.png", sourceImage);
            
            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            var resultImage = await content.OpenImageAsync("assets/test_ao.png");
            PixelAssert.RedEquals(value, resultImage);
        }

        [InlineData(  0,  0.0f, 255)]
        [InlineData(255,  0.0f, 255)]
        [InlineData(100,  1.0f, 155)]
        [InlineData(100,  0.5f, 205)]
        [InlineData(100,  2.0f,  55)]
        [InlineData(100,  3.0f,   0)]
        [InlineData(200, 0.01f, 253)]
        [Theory] public async Task ScaleValue(byte value, decimal scale, byte expected)
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
                    Occlusion = new MaterialOcclusionProperties {
                        Value = value,
                        Scale = scale,
                    },
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            var image = await content.OpenImageAsync("assets/test_ao.png");
            PixelAssert.RedEquals(expected, image);
        }

        [InlineData(  0,  0.0f, 255)]
        [InlineData(255,  0.0f, 255)]
        [InlineData(100,  1.0f, 100)]
        [InlineData(155,  0.5f, 205)]
        [InlineData(155,  2.0f,  55)]
        [InlineData(155,  3.0f,   0)]
        [InlineData( 55, 0.01f, 253)]
        [Theory] public async Task ScaleTexture(byte value, decimal scale, byte expected)
        {
            await using var provider = Builder.Build();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            var content = provider.GetRequiredService<MockFileContent>();
            graphBuilder.UseGlobalOutput = true;

            using var sourceImage = CreateImageR(value);
            await content.AddAsync("assets/test/occlusion.png", sourceImage);

            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                    Occlusion = new MaterialOcclusionProperties {
                        Scale = scale,
                    },
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            var image = await content.OpenImageAsync("assets/test_ao.png");
            PixelAssert.RedEquals(expected, image);
        }
    }
}
