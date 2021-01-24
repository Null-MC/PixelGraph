using PixelGraph.Common;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.ImageProcessors;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using PixelGraph.Tests.Internal;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PixelGraph.Tests.GenerationTests
{
    public class NormalGenerationTests : ImageTestBase
    {
        private readonly ResourcePackInputProperties packInput;
        private readonly ResourcePackProfileProperties packProfile;


        public NormalGenerationTests(ITestOutputHelper output) : base(output)
        {
            packInput = new ResourcePackInputProperties {
                Height = {
                    Texture = TextureTags.Height,
                    Color = ColorChannel.Red,
                },
            };

            packProfile = new ResourcePackProfileProperties {
                Encoding = {
                    NormalX = {
                        Texture = TextureTags.Normal,
                        Color = ColorChannel.Red,
                    },
                    NormalY = {
                        Texture = TextureTags.Normal,
                        Color = ColorChannel.Green,
                    },
                    NormalZ = {
                        Texture = TextureTags.Normal,
                        Color = ColorChannel.Blue,
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
            var context = new MaterialContext {
                Input = packInput,
                Profile = packProfile,
                Material = new MaterialProperties {
                    Name = "test",
                    LocalPath = "assets",
                },
            };

            await using var graph = Graph(context);
            await graph.CreateImageAsync("assets/test/height.png", height, 0, 0);
            await graph.ProcessAsync();

            using var image = await graph.GetImageAsync("assets/test_n.png");
            PixelAssert.RedEquals(127, image);
            PixelAssert.GreenEquals(127, image);
            PixelAssert.BlueEquals(255, image);
        }

        [Fact(Skip = "manual")]
        public async Task GenerateSobel3HighPass()
        {
            // Load Height Image
            var heightFile = PathEx.Join(AssemblyPath, "Data", "output-height.png");
            using var heightImage = await Image.LoadAsync<Rgba32>(Configuration.Default, heightFile);

            var srcWidth = heightImage.Width;
            var srcHeight = heightImage.Height;

            var options = new NormalMapProcessor.Options {
                Source = heightImage,
                HeightChannel = ColorChannel.Red,
                Filter = NormalMapFilters.SobelHigh,
                Strength = 1.0f,
                WrapX = true,
                WrapY = true,
            };

            var processor = new NormalMapProcessor(options);
            using var normalImage = new Image<Rgb24>(Configuration.Default, srcWidth, srcHeight);
            normalImage.Mutate(c => c.ApplyProcessor(processor));

            await SaveImageAsync("Output/output-sobel3-high.png", normalImage);
        }

        [Fact(Skip = "manual")]
        public async Task GenerateSobel3LowPass()
        {
            // Load Height Image
            var heightFile = PathEx.Join(AssemblyPath, "Data", "output-height.png");
            using var heightImage = await Image.LoadAsync<Rgba32>(Configuration.Default, heightFile);

            var srcWidth = heightImage.Width;
            var srcHeight = heightImage.Height;

            var options = new NormalMapProcessor.Options {
                Source = heightImage,
                HeightChannel = ColorChannel.Red,
                Filter = NormalMapFilters.SobelLow,
                Strength = 0.4f,
                WrapX = true,
                WrapY = true,
            };

            var processor = new NormalMapProcessor(options);
            using var normalImage = new Image<Rgb24>(Configuration.Default, srcWidth, srcHeight);
            normalImage.Mutate(c => c.ApplyProcessor(processor));

            await SaveImageAsync("Output/output-sobel3-low.png", normalImage);
        }

        [Fact(Skip = "manual")]
        public async Task GenerateMultiFrequency()
        {
            var Filter = KnownResamplers.Bicubic;

            var stages = new[] {
                new NormalFrequency(1.00f,  1, 0.15f),
                new NormalFrequency(0.75f,  4, 0.40f),
                new NormalFrequency(0.5f, 16, 1.00f),
            };

            // Load Height Image
            var heightFile = PathEx.Join(AssemblyPath, "Data", "height.png");
            using var heightImage = await Image.LoadAsync<Rgba32>(Configuration.Default, heightFile);

            var srcWidth = heightImage.Width;
            var srcHeight = heightImage.Height;


            //using var normalFinalImage = new Image<Rgb24>(Configuration.Default, srcWidth, srcHeight);
            Image<Rgb24> finalImage = null;

            var i = 0;
            foreach (var stage in stages.OrderByDescending(s => s.DownScale)) {
                i++;
                using var stageHeightImage = heightImage.Clone();

                int stageWidth, stageHeight;
                if (stage.DownScale > 1) {
                    stageWidth = (int)Math.Ceiling((double)srcWidth / stage.DownScale);
                    stageHeight = (int)Math.Ceiling((double)srcHeight / stage.DownScale);
                    stageHeightImage.Mutate(context => context.Resize(stageWidth, stageHeight, Filter));
                }
                else {
                    stageWidth = srcWidth;
                    stageHeight = srcHeight;
                }

                var options = new NormalMapProcessor.Options {
                    Source = stageHeightImage,
                    HeightChannel = ColorChannel.Red,
                    Strength = stage.Strength,
                    WrapX = true,
                    WrapY = true,
                };

                var processor = new NormalMapProcessor(options);

                using var stageNormalImage = new Image<Rgb24>(Configuration.Default, stageWidth, stageHeight);
                stageNormalImage.Mutate(c => c.ApplyProcessor(processor));

                if (stage.DownScale > 1)
                    stageNormalImage.Mutate(context => context.Resize(srcWidth, srcHeight, Filter));

                await SaveImageAsync($"Output/normal-stage-{i}.png", stageNormalImage);

                if (finalImage == null) finalImage = stageNormalImage.Clone();
                else {
                    // blend over final
                    var blendOptions = new NormalBlendProcessor.Options {
                        BlendImage = stageNormalImage,
                        Blend = stage.Blend,
                    };

                    var blendProcessor = new NormalBlendProcessor(blendOptions);
                    finalImage.Mutate(c => c.ApplyProcessor(blendProcessor));
                }

            }
            
            await SaveImageAsync("Output/normal-final.png", finalImage);
        }

        private Task SaveImageAsync(string localFile, Image image)
        {
            var filename = PathEx.Join(AssemblyPath, localFile);

            var path = Path.GetDirectoryName(filename);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            return image.SaveAsync(filename);
        }
    }

    internal class NormalFrequency
    {
        public float Strength {get; set;}
        public float Blend {get; set;}
        public int DownScale {get; set;}


        public NormalFrequency(float strength, int downScale, float blend)
        {
            Strength = strength;
            DownScale = downScale;
            Blend = blend;
        }
    }
}
