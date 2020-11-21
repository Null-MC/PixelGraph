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
    public class SubSurfaceScatteringTests : TestBase
    {
        private readonly ResourcePackProfileProperties packProfile;


        public SubSurfaceScatteringTests(ITestOutputHelper output) : base(output)
        {
            packProfile = new ResourcePackProfileProperties {
                Output = {
                    SSS = {
                        Red = EncodingChannel.SubSurfaceScattering,
                        Include = true,
                    },
                },
            };
        }

        [InlineData(  0,  0.0,   0)]
        [InlineData(100,  1.0, 100)]
        [InlineData(100,  0.5,  50)]
        [InlineData(100,  2.0, 200)]
        [InlineData(100,  3.0, 255)]
        [InlineData(200, 0.01,   2)]
        [Theory] public async Task Scale(byte value, decimal scale, byte expected)
        {
            await using var provider = Builder.Build();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            graphBuilder.UseGlobalOutput = true;

            var context = new MaterialContext {
                Input = new ResourcePackInputProperties {
                    Format = TextureEncoding.Format_Raw,
                },
                Profile = packProfile,
                Material = {
                    Name = "test",
                    LocalPath = "assets",
                    SSS = {
                        Value = value,
                        Scale = scale,
                    },
                },
            };

            await graphBuilder.ProcessOutputGraphAsync(context);
            var image = Content.Get<Image<Rgba32>>("assets/test_sss.png");
            PixelAssert.RedEquals(expected, image);
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

            using var heightImage = CreateImageR(value);
            Content.Add("assets/test/sss.png", heightImage);

            var context = new MaterialContext {
                Input = new ResourcePackInputProperties {
                    Format = TextureEncoding.Format_Raw,
                },
                Profile = packProfile,
                Material = {
                    Name = "test",
                    LocalPath = "assets",
                    SSS = {
                        Input = {
                            Red = EncodingChannel.SubSurfaceScattering,
                        },
                    },
                },
            };

            await graphBuilder.ProcessOutputGraphAsync(context);
            var image = Content.Get<Image<Rgba32>>("assets/test_sss.png");
            PixelAssert.RedEquals(value, image);
        }
    }
}
