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
    public class AlbedoTests : TestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public AlbedoTests(ITestOutputHelper output) : base(output)
        {
            packInput = new ResourcePackInputProperties {
                AlbedoRed = {
                    Texture = TextureTags.Albedo,
                    Color = ColorChannel.Red,
                    MaxValue = 255m,
                },
                AlbedoGreen = {
                    Texture = TextureTags.Albedo,
                    Color = ColorChannel.Green,
                    MaxValue = 255m,
                },
                AlbedoBlue = {
                    Texture = TextureTags.Albedo,
                    Color = ColorChannel.Blue,
                    MaxValue = 255m,
                },
            };

            packProfile = new ResourcePackProfileProperties {
                Encoding = {
                    AlbedoRed = {
                        Texture = TextureTags.Albedo,
                        Color = ColorChannel.Red,
                        MaxValue = 255m,
                    },
                    AlbedoGreen = {
                        Texture = TextureTags.Albedo,
                        Color = ColorChannel.Green,
                        MaxValue = 255m,
                    },
                    AlbedoBlue = {
                        Texture = TextureTags.Albedo,
                        Color = ColorChannel.Blue,
                        MaxValue = 255m,
                    },
                },
            };
        }

        [InlineData(  0)]
        [InlineData(100)]
        [InlineData(155)]
        [InlineData(255)]
        [Theory] public async Task RedPassthrough(byte value)
        {
            await using var provider = Builder.Build();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            var content = provider.GetRequiredService<MockFileContent>();
            graphBuilder.UseGlobalOutput = true;

            using var sourceImage = CreateImage(value, 0, 0);
            await content.AddAsync("assets/test/albedo.png", sourceImage);

            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            using var resultImage = await content.OpenImageAsync("assets/test.png");
            PixelAssert.RedEquals(value, resultImage);
        }

        [InlineData(  0)]
        [InlineData(100)]
        [InlineData(155)]
        [InlineData(255)]
        [Theory] public async Task GreenPassthrough(byte value)
        {
            await using var provider = Builder.Build();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            var content = provider.GetRequiredService<MockFileContent>();
            graphBuilder.UseGlobalOutput = true;

            using var sourceImage = CreateImage(0, value, 0);
            await content.AddAsync("assets/test/albedo.png", sourceImage);

            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            using var resultImage = await content.OpenImageAsync("assets/test.png");
            PixelAssert.GreenEquals(value, resultImage);
        }

        [InlineData(  0, 0.00,   0)]
        [InlineData(100, 1.00, 100)]
        [InlineData(100, 0.50,  50)]
        [InlineData(100, 2.00, 200)]
        [InlineData(100, 3.00, 255)]
        [InlineData(200, 0.01,   2)]
        [Theory] public async Task ScaleRedValue(decimal value, decimal scale, byte expected)
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
                    Albedo = new MaterialAlbedoProperties {
                        ValueRed = value,
                        ScaleRed = scale,
                    },
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            using var image = await content.OpenImageAsync("assets/test.png");
            PixelAssert.RedEquals(expected, image);
        }

        [InlineData(  0, 0.0,   0)]
        [InlineData(100, 1.0, 100)]
        [InlineData(100, 0.5,  50)]
        [InlineData(100, 2.0, 200)]
        [InlineData(100, 3.0, 255)]
        [InlineData(200, 0.01,  2)]
        [Theory] public async Task ScaleRedTexture(byte value, decimal scale, byte expected)
        {
            await using var provider = Builder.Build();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            var content = provider.GetRequiredService<MockFileContent>();
            graphBuilder.UseGlobalOutput = true;

            using var srcImage = CreateImageR(value);
            await content.AddAsync("assets/test/albedo.png", srcImage);

            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                    Albedo = new MaterialAlbedoProperties {
                        ScaleRed = scale,
                    },
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            using var image = await content.OpenImageAsync("assets/test.png");
            PixelAssert.RedEquals(expected, image);
        }

        [Fact]
        public async Task ShiftRgb()
        {
            await using var provider = Builder.Build();
            var graphBuilder = provider.GetRequiredService<ITextureGraphBuilder>();
            var content = provider.GetRequiredService<MockFileContent>();
            graphBuilder.UseGlobalOutput = true;

            using var image = CreateImage(60, 120, 180);
            await content.AddAsync("assets/test/albedo.png", image);

            var context = new MaterialContext {
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                },
                Input = new ResourcePackInputProperties {
                    AlbedoRed = {
                        Texture = TextureTags.Albedo,
                        Color = ColorChannel.Green,
                        MaxValue = 255m,
                    },
                    AlbedoGreen = {
                        Texture = TextureTags.Albedo,
                        Color = ColorChannel.Blue,
                        MaxValue = 255m,
                    },
                    AlbedoBlue = {
                        Texture = TextureTags.Albedo,
                        Color = ColorChannel.Red,
                        MaxValue = 255m,
                    },
                },
            };

            await graphBuilder.ProcessInputGraphAsync(context);
            using var outputImage = await content.OpenImageAsync("assets/test.png");
            PixelAssert.RedEquals(120, outputImage);
            PixelAssert.GreenEquals(180, outputImage);
            PixelAssert.BlueEquals(60, outputImage);
        }
    }
}
