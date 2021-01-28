using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.Encoding;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.Tests.Internal;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.EncodingChannelTests
{
    public class PorositySSSTests : TestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public PorositySSSTests(ITestOutputHelper output) : base(output)
        {
            packInput = new ResourcePackInputProperties {
                //Format = TextureEncoding.Format_Raw,
            };

            packProfile = new ResourcePackProfileProperties {
                Output = {
                    Specular = new TextureOutputEncoding {
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
            var content = provider.GetRequiredService<MockFileContent>();
            graphBuilder.UseGlobalOutput = true;

            using var heightImage = CreateImageR(value);
            await content.AddAsync("assets/test/specular.png", heightImage);

            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                    Specular = new MaterialSpecularProperties {
                        Input = new TextureEncoding {
                            Red = EncodingChannel.Porosity_SSS,
                        },
                    },
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);

            var image = await content.OpenImageAsync("assets/test_s.png");
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
            var content = provider.GetRequiredService<MockFileContent>();
            graphBuilder.UseGlobalOutput = true;

            using var porosityImage = CreateImageR(value);
            await content.AddAsync("assets/test/porosity.png", porosityImage);

            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                    Porosity = new MaterialPorosityProperties {
                        Input = new TextureEncoding {
                            Red = EncodingChannel.Porosity,
                        },
                    },
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            var image = await content.OpenImageAsync("assets/test_s.png");
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
            var content = provider.GetRequiredService<MockFileContent>();
            graphBuilder.UseGlobalOutput = true;

            using var porosityImage = CreateImageR(value);
            await content.AddAsync("assets/test/sss.png", porosityImage);

            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                    SSS = new MaterialSssProperties {
                        Input = new TextureEncoding {
                            Red = EncodingChannel.SubSurfaceScattering,
                        },
                    },
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            var image = await content.OpenImageAsync("assets/test_s.png");
            PixelAssert.RedEquals(expected, image);
        }
    }
}
