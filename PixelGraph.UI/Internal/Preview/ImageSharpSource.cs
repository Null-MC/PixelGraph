using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixelGraph.UI.Internal.Preview;

public abstract class ImageSharpSource : BitmapSource
{
    public Type SourceFormat {get; set;}
}

/// <remarks> https://github.com/jongleur1983/SharpImageSource </remarks>
public sealed class ImageSharpSource<TPixel> : ImageSharpSource
    where TPixel : unmanaged, IPixel<TPixel>
{
    private readonly Image<TPixel> source;

    //public override event EventHandler<ExceptionEventArgs> DecodeFailed;
    //public override event EventHandler<DownloadProgressEventArgs> DownloadProgress;
    //public override event EventHandler<ExceptionEventArgs> DownloadFailed;
    //public override event EventHandler DownloadCompleted;

    public override PixelFormat Format => PixelFormats.Bgra32;
    public override int PixelHeight => source.Height;
    public override int PixelWidth => source.Width;
    public override double DpiX => source.Metadata.HorizontalResolution;
    public override double DpiY => source.Metadata.VerticalResolution;
    public override BitmapPalette Palette => null;
    public override bool IsDownloading => false;


    private ImageSharpSource()
    {
        SourceFormat = typeof(TPixel);
    }

    public ImageSharpSource(Image<TPixel> source) : this()
    {
        this.source = source;
    }

    private ImageSharpSource(int width, int height) : this()
    {
        source = new Image<TPixel>(width, height);
    }

    public override void CopyPixels(Array pixels, int stride, int offset)
    {
        var sourceRect = new Int32Rect(0, 0, PixelWidth, PixelHeight);
        base.CopyPixels(sourceRect, pixels, stride, offset);
    }

    public override void CopyPixels(Int32Rect sourceRect, Array pixels, int stride, int offset)
    {
        ValidateArrayAndGetInfo(pixels, out var elementSize, out var bufferSize, out var elementType);

        if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));

        // We accept arrays of arbitrary value types - but not reference types.
        if (elementType is not { IsValueType: true })
            throw new ArgumentException("must be a valueType!", nameof(pixels));

        checked {
            var offsetInBytes = offset * elementSize;

            if (offsetInBytes >= bufferSize)
                throw new IndexOutOfRangeException();

            // Get the address of the data in the array by pinning it.
            var arrayHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);

            try {
                // Adjust the buffer and bufferSize to account for the offset.
                var buffer = arrayHandle.AddrOfPinnedObject();
                buffer = new IntPtr((long)buffer + offsetInBytes);
                bufferSize -= offsetInBytes;

                CopyPixels(sourceRect, buffer, bufferSize, stride);
            }
            finally {
                arrayHandle.Free();
            }
        }
    }

    public override void CopyPixels(Int32Rect sourceRect, IntPtr buffer, int bufferSize, int stride)
    {
        // WIC would specify NULL for the source rect to indicate that the
        // entire content should be copied.  WPF turns that into an empty
        // rect, which we inflate here to be the entire bounds.
        if (sourceRect.IsEmpty) {
            sourceRect.Width = PixelWidth;
            sourceRect.Height = PixelHeight;
        }

        if (sourceRect.X < 0 || sourceRect.Width < 0 || sourceRect.Y < 0 || sourceRect.Height < 0 || sourceRect.X + sourceRect.Width > PixelWidth || sourceRect.Y + sourceRect.Height > PixelHeight)
            throw new ArgumentOutOfRangeException(nameof(sourceRect));

        if (buffer == IntPtr.Zero) throw new ArgumentNullException(nameof(buffer));

        checked {
            if (stride < 1) throw new ArgumentOutOfRangeException(nameof(stride));

            var minStrideInBits = (uint)(sourceRect.Width * Format.BitsPerPixel);
            var minStrideInBytes = (minStrideInBits + 7) / 8;

            if (stride < minStrideInBytes)
                throw new ArgumentOutOfRangeException(nameof(stride));

            if (bufferSize < 1)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            var minBufferSize = (uint)((sourceRect.Height - 1) * stride) + minStrideInBytes;

            if (bufferSize < minBufferSize)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
        }

        CopyPixelsCore(ref sourceRect, in stride, ref buffer);
    }

    protected override Freezable CreateInstanceCore()
    {
        return new ImageSharpSource<TPixel>(1, 1);
    }

    private void CopyPixelsCore(ref Int32Rect sourceRect, in int stride, ref IntPtr buffer)
    {
        if (source == null) return;

        unsafe {
            var pBytes = (byte*)buffer.ToPointer();

            for (var y = 0; y < sourceRect.Height; y++) {
                var pPixel = (Bgra32*)pBytes;

                for (var x = 0; x < sourceRect.Width; x++) {
                    var dest = default(Rgba32);
                    source[x + sourceRect.X, y + sourceRect.Y].ToRgba32(ref dest);

                    // Write sRGB (non-linear) since it is implied by
                    // the pixel format we chose.
                    pPixel->G = dest.G;
                    pPixel->R = dest.R;
                    pPixel->B = dest.B;
                    pPixel->A = dest.A;

                    pPixel++;
                }

                pBytes += stride;
            }
        }
    }

    private static void ValidateArrayAndGetInfo(Array pixels, out int elementSize, out int sourceBufferSize, out Type elementType)
    {
        if (pixels == null) throw new ArgumentNullException(nameof(pixels));

        switch (pixels.Rank) {
            case 1: {
                var len0 = pixels.GetLength(0);
                if (len0 <= 0) throw new ArgumentOutOfRangeException(nameof(pixels));

                checked {
                    var exemplar = pixels.GetValue(0);
                    if (exemplar == null) throw new ArgumentNullException(nameof(pixels));

                    elementSize = Marshal.SizeOf(exemplar);
                    sourceBufferSize = len0 * elementSize;
                    elementType = exemplar.GetType();
                }
                break;
            }
            case 2: {
                var len0 = pixels.GetLength(0);
                var len1 = pixels.GetLength(1);
                if (len0 <= 0 || len1 <= 0) throw new ArgumentOutOfRangeException(nameof(pixels));

                checked {
                    var exemplar = pixels.GetValue(0, 0);
                    if (exemplar == null) throw new ArgumentNullException(nameof(pixels));

                    elementSize = Marshal.SizeOf(exemplar);
                    sourceBufferSize = len0 * len1 * elementSize;
                    elementType = exemplar.GetType();
                }
                break;
            }
            default:
                throw new ArgumentException($"Invalid pixel rank '{pixels.Rank}'!", nameof(pixels));
        }
    }
}