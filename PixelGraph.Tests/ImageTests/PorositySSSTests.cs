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
    public class PorositySSSTests : TestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public PorositySSSTests(ITestOutputHelper output) : base(output)
        {
            packInput = new ResourcePackInputProperties {
                Format = TextureEncoding.Format_Raw,
            };

            packProfile = new ResourcePackProfileProperties {
                Output = {
                    Specular = {
                        Red = EncodingChannel.Porosity_SSS,
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

            using var heightImage = CreateImageR(value);
            Content.Add("assets/test/specular.png", heightImage);

            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = {
                    Name = "test",
                    LocalPath = "assets",
                    Specular = {
                        Input = {
                            Red = EncodingChannel.Porosity_SSS,
                        },
                    },
                },
            };

            await graphBuilder.ProcessOutputGraphAsync(context);

            var image = Content.Get<Image<Rgba32>>("assets/test_s.png");
            PixelAssert.RedEquals(value, image);
        }

        [InlineData(  0,  0)]
        [InlineData(100, 25)]
        [InlineData(200, 50)]
        [InlineData(255, 64)]
        [Theory] public async Task ConvertsPorosityToPSSS(byte value, byte expected)
        {
            await using var provider = Builder.Build();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            graphBuilder.UseGlobalOutput = true;

            using var porosityImage = CreateImageR(value);
            Content.Add("assets/test/porosity.png", porosityImage);

            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = {
                    Name = "test",
                    LocalPath = "assets",
                    Porosity = {
                        Input = {
                            Red = EncodingChannel.Porosity,
                        },
                    },
                },
            };

            await graphBuilder.ProcessOutputGraphAsync(context);
            var image = Content.Get<Image<Rgba32>>("assets/test_s.png");
            PixelAssert.RedEquals(expected, image);
        }

        [InlineData(  0,  65)]
        [InlineData(100, 140)]
        [InlineData(200, 214)]
        [InlineData(253, 254)]
        [InlineData(255, 255)]
        [Theory] public async Task ConvertsSSSToPSSS(byte value, byte expected)
        {
            await using var provider = Builder.Build();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            graphBuilder.UseGlobalOutput = true;

            using var porosityImage = CreateImageR(value);
            Content.Add("assets/test/sss.png", porosityImage);

            var context = new MaterialContext {
                Input = packInput,
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
            var image = Content.Get<Image<Rgba32>>("assets/test_s.png");
            PixelAssert.RedEquals(expected, image);
        }
    }
}
