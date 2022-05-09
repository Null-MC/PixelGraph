using PixelGraph.Common;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.Common.Textures.Graphing;
using PixelGraph.Tests.Internal;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.GenerationTests
{
    public class NormalGenerationTests : ImageTestBase
    {
        private readonly ProjectData project;
        private readonly PublishProfileProperties packProfile;


        public NormalGenerationTests(ITestOutputHelper output) : base(output)
        {
            Builder.ConfigureReader(ContentTypes.File, GameEditions.None, null);
            Builder.ConfigureWriter(ContentTypes.File, GameEditions.None, null);

            project = new ProjectData {
                Input = new PackInputEncoding {
                    Height = {
                        Texture = TextureTags.Height,
                        Color = ColorChannel.Red,
                    },
                },
            };

            packProfile = new PublishProfileProperties {
                Encoding = {
                    NormalX = {
                        Texture = TextureTags.Normal,
                        Color = ColorChannel.Red,
                        MinValue = -1m,
                        MaxValue = 1m,
                        DefaultValue = 0m,
                    },
                    NormalY = {
                        Texture = TextureTags.Normal,
                        Color = ColorChannel.Green,
                        MinValue = -1m,
                        MaxValue = 1m,
                        DefaultValue = 0m,
                    },
                    NormalZ = {
                        Texture = TextureTags.Normal,
                        Color = ColorChannel.Blue,
                        MinValue = -1m,
                        MaxValue = 1m,
                        DefaultValue = 1m,
                    },
                },
            };
        }

        [InlineData(  0)]
        [InlineData( 50)]
        [InlineData(100)]
        [InlineData(200)]
        [InlineData(255)]
        [Theory] public async Task GeneratesFlat(byte height)
        {
            await using var graph = Graph();

            graph.Project = project;
            graph.PackProfile = packProfile;
            graph.Material = new MaterialProperties {
                Name = "test",
                LocalPath = "assets",
            };

            await graph.CreateImageAsync("assets/test/height.png", height);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync<Rgb24>("assets/test_n.png");
            PixelAssert.RedEquals(128, image);
            PixelAssert.GreenEquals(128, image);
            PixelAssert.BlueEquals(255, image);
        }

        [Fact(Skip = "manual")]
        public async Task GenerateSobel3HighPass()
        {
            var heightFile = PathEx.Join(AssemblyPath, "Data", "output-height.png");
            using var heightImage = await Image.LoadAsync<L8>(Configuration.Default, heightFile);

            var options = new NormalMapProcessor<L8>.Options {
                Source = heightImage,
                HeightChannel = ColorChannel.Red,
                Method = NormalMapMethods.SobelHigh,
                Strength = 1.0f,
                WrapX = true,
                WrapY = true,
            };

            var processor = new NormalMapProcessor<L8>(options);
            using var normalImage = new Image<Rgb24>(Configuration.Default, heightImage.Width, heightImage.Height);
            normalImage.Mutate(c => c.ApplyProcessor(processor));

            await SaveImageAsync("Output/output-sobel3-high.png", normalImage);
        }

        [Fact(Skip = "manual")]
        public async Task GenerateSobel3LowPass()
        {
            var heightFile = PathEx.Join(AssemblyPath, "Data", "output-height.png");
            using var heightImage = await Image.LoadAsync<L8>(Configuration.Default, heightFile);

            var options = new NormalMapProcessor<L8>.Options {
                Source = heightImage,
                HeightChannel = ColorChannel.Red,
                Method = NormalMapMethods.SobelLow,
                Strength = 0.4f,
                WrapX = true,
                WrapY = true,
            };

            var processor = new NormalMapProcessor<L8>(options);
            using var normalImage = new Image<Rgb24>(Configuration.Default, heightImage.Width, heightImage.Height);
            normalImage.Mutate(c => c.ApplyProcessor(processor));

            await SaveImageAsync("Output/output-sobel3-low.png", normalImage);
        }

        [Fact(Skip = "manual")]
        public async Task GenerateVariance()
        {
            var heightFile = PathEx.Join(AssemblyPath, "Data", "height.png");
            using var heightImage = await Image.LoadAsync<L8>(Configuration.Default, heightFile);

            var context = new TextureGraphContext();
            var regions = new TextureRegionEnumerator(context);

            using var builder = new NormalMapBuilder<L8>(regions) {
                HeightImage = heightImage,
                HeightChannel = ColorChannel.Red,
                Method = NormalMapMethods.Variance,
                LowFreqDownscale = 4,
                VarianceBlur = 3f,
                VarianceStrength = 0.997f,
                LowFreqStrength = 2f,
                Strength = 2f,
                WrapX = false,
                WrapY = false,
            };

            using var normalImage = builder.Build();
            await SaveImageAsync("Output/final.png", normalImage);
            await SaveImageAsync("Output/variance.png", builder.VarianceMap);
        }

        private Task SaveImageAsync(string localFile, Image image)
        {
            var filename = PathEx.Join(AssemblyPath, localFile);

            var path = Path.GetDirectoryName(filename);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            return image.SaveAsync(filename);
        }
    }
}
