using PixelGraph.Common.ImageProcessors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PixelGraph.Common.Textures;

internal class NormalMapBuilder<THeight> : IDisposable
    where THeight : unmanaged, IPixel<THeight>
{
    private readonly TextureRegionEnumerator regions;

    public NormalMapMethods Method {get; set;}
    public Image<THeight>? HeightImage {get; set;}
    public ColorChannel HeightChannel {get; set;}
    public float Strength {get; set;}
    public bool WrapX {get; set;}
    public bool WrapY {get; set;}

    public float LowFreqStrength {get; set;}
    public float LowFreqDownscale {get; set;}
    public float VarianceStrength {get; set;}
    public float VarianceBlur {get; set;}

    public Image<L8>? VarianceMap {get; private set;}


    public NormalMapBuilder(TextureRegionEnumerator regions)
    {
        this.regions = regions;
    }

    public void Dispose()
    {
        VarianceMap?.Dispose();
    }

    public Image<Rgb24> Build()
    {
        return Method == NormalMapMethods.Variance
            ? BuildVariance()
            : BuildSimple();
    }

    private Image<Rgb24> BuildSimple()
    {
        ArgumentNullException.ThrowIfNull(HeightImage);

        var options = new NormalMapProcessor<THeight>.Options {
            Source = HeightImage,
            HeightChannel = HeightChannel,
            Strength = MathF.Max(Strength, float.Epsilon),
            Method = Method,
            WrapX = WrapX,
            WrapY = WrapY,
        };
            
        Image<Rgb24>? resultImage = null;

        try {
            var processor = new NormalMapProcessor<THeight>(options);

            resultImage = new Image<Rgb24>(Configuration.Default, HeightImage.Width, HeightImage.Height);

            foreach (var frame in regions.GetAllRenderRegions()) {
                if (frame.Tiles ==  null) continue;

                foreach (var tile in frame.Tiles) {
                    var outBounds = tile.DestBounds.ScaleTo(HeightImage.Width, HeightImage.Height);
                    //options.SourceBounds = ;
                        
                    resultImage.Mutate(c => c.ApplyProcessor(processor, outBounds));
                }
            }

            return resultImage;
        }
        catch {
            resultImage?.Dispose();
            throw;
        }
    }

    private Image<Rgb24> BuildVariance()
    {
        ArgumentNullException.ThrowIfNull(HeightImage);

        if (LowFreqDownscale <= 1f) throw new ArgumentOutOfRangeException(nameof(LowFreqDownscale), "Variance downscale must be greater than 1!");

        var lowFreqFilterDown = KnownResamplers.Box;
        var lowFreqFilterUp = KnownResamplers.Bicubic;
        var srcWidth = HeightImage.Width;
        var srcHeight = HeightImage.Height;

        VarianceMap?.Dispose();
        VarianceMap = new Image<L8>(Configuration.Default, srcWidth, srcHeight);
            
        // Make high-freq Normal map
        var normalHighFreqOptions = new NormalMapProcessor<THeight>.Options {
            Source = HeightImage,
            HeightChannel = HeightChannel,
            Strength = MathF.Max(Strength, float.Epsilon),
            WrapX = WrapX,
            WrapY = WrapY,
        };

        var normalHighFreqProcessor = new NormalMapProcessor<THeight>(normalHighFreqOptions);

        using var normalHighFreqImage = new Image<Rgb24>(Configuration.Default, srcWidth, srcHeight);
        normalHighFreqImage.Mutate(c => c.ApplyProcessor(normalHighFreqProcessor));

        // Create low-freq Height image
        var lowFreqWidth = (int)Math.Ceiling((double)srcWidth / LowFreqDownscale);
        var lowFreqHeight = (int)Math.Ceiling((double)srcHeight / LowFreqDownscale);

        using var heightLowFreqImage = HeightImage.Clone();
        heightLowFreqImage.Mutate(context => context.Resize(lowFreqWidth, lowFreqHeight, lowFreqFilterDown));

        // Make low-freq Normal map
        var normalLowFreqOptions = new NormalMapProcessor<THeight>.Options {
            Source = heightLowFreqImage,
            HeightChannel = HeightChannel,
            Strength = LowFreqStrength,
            WrapX = WrapX,
            WrapY = WrapY,
        };

        var normalLowFreqProcessor = new NormalMapProcessor<THeight>(normalLowFreqOptions);

        using var normalLowFreqImage = new Image<Rgb24>(Configuration.Default, lowFreqWidth, lowFreqHeight);
        normalLowFreqImage.Mutate(c => c.ApplyProcessor(normalLowFreqProcessor));

        // Upscale low-freq Height and Normal map
        heightLowFreqImage.Mutate(context => context.Resize(srcWidth, srcHeight, lowFreqFilterUp));
        normalLowFreqImage.Mutate(context => context.Resize(srcWidth, srcHeight, lowFreqFilterUp));

        // Create Height low/high Variance map
        var f = 1f / (1f - VarianceStrength + float.Epsilon);

        var varianceOptions = new HeightVarianceProcessor<THeight>.Options {
            HighFreqHeightImage = HeightImage,
            LowFreqHeightImage = heightLowFreqImage,
            HeightChannel = HeightChannel,
            Strength = f,
        };

        var varianceProcessor = new HeightVarianceProcessor<THeight>(varianceOptions);

        VarianceMap.Mutate(c => {
            c.ApplyProcessor(varianceProcessor);
            c.GaussianBlur(VarianceBlur);
        });

        // Merge high/low freq normals using variance
        var blendOptions = new NormalBlendProcessor.Options {
            HighFreqNormalImage = normalHighFreqImage,
            LowFreqNormalImage = normalLowFreqImage,
            VarianceImage = VarianceMap,
        };

        Image<Rgb24>? resultImage = null;

        try {
            var blendProcessor = new NormalBlendProcessor(blendOptions);

            resultImage = new Image<Rgb24>(Configuration.Default, srcWidth, srcHeight);

            foreach (var frame in regions.GetAllRenderRegions()) {
                if (frame.Tiles ==  null) continue;

                foreach (var tile in frame.Tiles) {
                    //sampler.Bounds = tile.Bounds;

                    var outBounds = tile.DestBounds.ScaleTo(srcWidth, srcHeight);
                    resultImage.Mutate(c => c.ApplyProcessor(blendProcessor, outBounds));
                }
            }
                
            return resultImage;
        }
        catch {
            resultImage?.Dispose();
            throw;
        }
    }
}