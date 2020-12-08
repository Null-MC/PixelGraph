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
    public class PerceptualSmoothTests : TestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public PerceptualSmoothTests(ITestOutputHelper output) : base(output)
        {
            packInput = new ResourcePackInputProperties {
                Smooth = {
                    Texture = TextureTags.Smooth,
                    Color = ColorChannel.Red,
                    Power = 0.5m,
                }
            };

            packProfile = new ResourcePackProfileProperties {
                Output = {
                    Smooth = {
                        Texture = TextureTags.Smooth,
                        Color = ColorChannel.Red,
                        Power = 0.5m,
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

            using var smoothImage = CreateImageR(value);
            await content.AddAsync("assets/test/smooth.png", smoothImage);

            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            var image = await content.OpenImageAsync("assets/test_smooth.png");
            PixelAssert.RedEquals(value, image);
        }

        [InlineData(  0,   0)]
        [InlineData(100, 160)]
        [InlineData(200, 226)]
        [InlineData(255, 255)]
        [Theory] public async Task ConvertsSmoothToPerceptualSmooth(byte value, byte expected)
        {
            await using var provider = Builder.Build();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            var content = provider.GetRequiredService<MockFileContent>();
            graphBuilder.UseGlobalOutput = true;

            using var smoothImage = CreateImageR(value);
            await content.AddAsync("assets/test/smooth.png", smoothImage);

            var context = new MaterialContext {
                Input = new ResourcePackInputProperties {
                    Smooth = {
                        Texture = TextureTags.Smooth,
                        Color = ColorChannel.Red,
                    },
                },
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            var image = await content.OpenImageAsync("assets/test_smooth.png");
            PixelAssert.RedEquals(expected, image);
        }
    }
}
