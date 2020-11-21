using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.Encoding;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.Tests.Internal;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
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
                Format = TextureEncoding.Format_Raw,
            };

            packProfile = new ResourcePackProfileProperties {
                Output = {
                    Smooth = {
                        Red = EncodingChannel.PerceptualSmooth,
                        Include = true,
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
            graphBuilder.UseGlobalOutput = true;

            using var smoothImage = CreateImageR(value);
            Content.Add("assets/test/smooth.png", smoothImage);

            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = {
                    Name = "test",
                    LocalPath = "assets",
                    Smooth = {
                        Input = {
                            Red = EncodingChannel.PerceptualSmooth,
                        },
                    },
                },
            };

            await graphBuilder.ProcessOutputGraphAsync(context);
            var image = Content.Get<Image<Rgba32>>("assets/test_smooth.png");
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
            graphBuilder.UseGlobalOutput = true;

            using var smoothImage = CreateImageR(value);
            Content.Add("assets/test/smooth.png", smoothImage);

            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = {
                    Name = "test",
                    LocalPath = "assets",
                    Smooth = {
                        Input = {
                            Red = EncodingChannel.Smooth,
                        },
                    },
                },
            };

            await graphBuilder.ProcessOutputGraphAsync(context);
            var image = Content.Get<Image<Rgba32>>("assets/test_smooth.png");
            PixelAssert.RedEquals(expected, image);
        }
    }
}
