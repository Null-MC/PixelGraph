using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Encoding;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixelGraph.UI.Internal
{
    internal interface ITexturePreviewBuilder : IDisposable
    {
        ResourcePackInputProperties Input {get; set;}
        MaterialProperties Material {get; set;}
        ResourcePackProfileProperties Profile {get; set;}
        CancellationToken Token {get;}

        Task<ImageSource> BuildAsync(string tag);
        void Cancel();
    }

    internal class TexturePreviewBuilder : ITexturePreviewBuilder
    {
        private readonly IServiceProvider provider;
        private readonly CancellationTokenSource tokenSource;

        public ResourcePackInputProperties Input {get; set;}
        public MaterialProperties Material {get; set;}
        public ResourcePackProfileProperties Profile {get; set;}

        public CancellationToken Token => tokenSource.Token;


        public TexturePreviewBuilder(IServiceProvider provider)
        {
            this.provider = provider;
            tokenSource = new CancellationTokenSource();
        }

        public async Task<ImageSource> BuildAsync(string tag)
        {
            var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();
            var graph = scope.ServiceProvider.GetRequiredService<ITextureGraph>();

            context.Input = Input;
            context.Profile = Profile;
            context.Material = Material;

            var inputFormat = TextureEncoding.GetFactory(Input?.Format);
            var inputEncoding = inputFormat?.Create() ?? new ResourcePackEncoding();
            inputEncoding.Merge(Input);
            inputEncoding.Merge(Material);
            context.InputEncoding = inputEncoding.GetMapped().ToList();

            if (tagMap.TryGetValue(tag, out var channelFunc)) {
                var channels = channelFunc(Profile);
                context.OutputEncoding.AddRange(channels);
            }

            if (TextureTags.Is(tag, TextureTags.Normal))
                await graph.PreBuildNormalTextureAsync(tokenSource.Token);

            await graph.MapAsync(tag, true, 0, Token);
            using var image = await graph.CreateImageAsync<Rgb24>(tag, true, tokenSource.Token);
            if (image == null) return null;

            if (image.Width > 1 || image.Height > 1) {
                if (context.Material.TryGetSourceBounds(out var bounds)) {
                    var scaleX = (float)image.Width / bounds.Width;

                    foreach (var region in context.Material.Parts) {
                        var regionBounds = region.GetRectangle();
                        regionBounds.X = (int)MathF.Ceiling(regionBounds.X * scaleX);
                        regionBounds.Y = (int)MathF.Ceiling(regionBounds.Y * scaleX);
                        regionBounds.Width = (int)MathF.Ceiling(regionBounds.Width * scaleX);
                        regionBounds.Height = (int)MathF.Ceiling(regionBounds.Height * scaleX);

                        graph.FixEdges(image, tag, regionBounds);
                    }
                }
                else {
                    graph.FixEdges(image, tag);
                }
            }

            return await CreateImageSourceAsync(image, tokenSource.Token);
        }

        public void Cancel()
        {
            tokenSource.Cancel();
        }

        public void Dispose()
        {
            tokenSource?.Dispose();
        }

        private static async Task<ImageSource> CreateImageSourceAsync(Image image, CancellationToken token)
        {
            await using var stream = new MemoryStream();
            await image.SaveAsync(stream, PngFormat.Instance, token);
            await stream.FlushAsync(token);
            stream.Seek(0, SeekOrigin.Begin);

            var imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.CacheOption = BitmapCacheOption.OnLoad;
            imageSource.StreamSource = stream;
            imageSource.EndInit();
            imageSource.Freeze();

            return imageSource;
        }

        private static readonly Dictionary<string, Func<ResourcePackProfileProperties, ResourcePackChannelProperties[]>> tagMap =
            new Dictionary<string, Func<ResourcePackProfileProperties, ResourcePackChannelProperties[]>>(StringComparer.InvariantCultureIgnoreCase) {
                [TextureTags.Alpha] = profile => new ResourcePackChannelProperties[] {
                    new ResourcePackAlphaChannelProperties(TextureTags.Alpha, ColorChannel.Red) {
                        Sampler = profile?.Encoding?.Alpha?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackAlphaChannelProperties(TextureTags.Alpha, ColorChannel.Green) {
                        Sampler = profile?.Encoding?.Alpha?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackAlphaChannelProperties(TextureTags.Alpha, ColorChannel.Blue) {
                        Sampler = profile?.Encoding?.Alpha?.Sampler,
                        MaxValue = 255,
                    },
                },
                [TextureTags.Diffuse] = profile => new ResourcePackChannelProperties[] {
                    new ResourcePackDiffuseRedChannelProperties(TextureTags.Diffuse, ColorChannel.Red) {
                        Sampler = profile?.Encoding?.DiffuseRed?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackDiffuseGreenChannelProperties(TextureTags.Diffuse, ColorChannel.Green) {
                        Sampler = profile?.Encoding?.DiffuseGreen?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackDiffuseBlueChannelProperties(TextureTags.Diffuse, ColorChannel.Blue) {
                        Sampler = profile?.Encoding?.DiffuseBlue?.Sampler,
                        MaxValue = 255,
                    },
                },
                [TextureTags.Albedo] = profile => new ResourcePackChannelProperties[] {
                    new ResourcePackAlbedoRedChannelProperties(TextureTags.Albedo, ColorChannel.Red) {
                        Sampler = profile?.Encoding?.AlbedoRed?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackAlbedoGreenChannelProperties(TextureTags.Albedo, ColorChannel.Green) {
                        Sampler = profile?.Encoding?.AlbedoGreen?.Sampler,
                        MaxValue = 255,
                    },
                    new ResourcePackAlbedoBlueChannelProperties(TextureTags.Albedo, ColorChannel.Blue) {
                        Sampler = profile?.Encoding?.AlbedoBlue?.Sampler,
                        MaxValue = 255,
                    },
                },
                [TextureTags.Height] = profile => new ResourcePackChannelProperties[] {
                    new ResourcePackHeightChannelProperties(TextureTags.Height, ColorChannel.Red) {
                        Sampler = profile?.Encoding?.Height?.Sampler,
                        Invert = true,
                    },
                    new ResourcePackHeightChannelProperties(TextureTags.Height, ColorChannel.Green) {
                        Sampler = profile?.Encoding?.Height?.Sampler,
                        Invert = true,
                    },
                    new ResourcePackHeightChannelProperties(TextureTags.Height, ColorChannel.Blue) {
                        Sampler = profile?.Encoding?.Height?.Sampler,
                        Invert = true,
                    },
                },
                [TextureTags.Occlusion] = profile => new ResourcePackChannelProperties[] {
                    new ResourcePackOcclusionChannelProperties(TextureTags.Occlusion, ColorChannel.Red), //{Invert = true};
                    new ResourcePackOcclusionChannelProperties(TextureTags.Occlusion, ColorChannel.Green), //{Invert = true};
                    new ResourcePackOcclusionChannelProperties(TextureTags.Occlusion, ColorChannel.Blue), //{Invert = true};
                },
                [TextureTags.Normal] = profile => new ResourcePackChannelProperties[] {
                    new ResourcePackNormalXChannelProperties(TextureTags.Normal, ColorChannel.Red),
                    new ResourcePackNormalYChannelProperties(TextureTags.Normal, ColorChannel.Green),
                    new ResourcePackNormalZChannelProperties(TextureTags.Normal, ColorChannel.Blue),
                },
                [TextureTags.Specular] = profile => new ResourcePackChannelProperties[] {
                    new ResourcePackSpecularChannelProperties(TextureTags.Specular, ColorChannel.Red),
                    new ResourcePackSpecularChannelProperties(TextureTags.Specular, ColorChannel.Green),
                    new ResourcePackSpecularChannelProperties(TextureTags.Specular, ColorChannel.Blue),
                },
                [TextureTags.Smooth] = profile => new ResourcePackChannelProperties[] {
                    new ResourcePackSmoothChannelProperties(TextureTags.Smooth, ColorChannel.Red),
                    new ResourcePackSmoothChannelProperties(TextureTags.Smooth, ColorChannel.Green),
                    new ResourcePackSmoothChannelProperties(TextureTags.Smooth, ColorChannel.Blue),
                },
                [TextureTags.Rough] = profile => new ResourcePackChannelProperties[] {
                    new ResourcePackRoughChannelProperties(TextureTags.Rough, ColorChannel.Red),
                    new ResourcePackRoughChannelProperties(TextureTags.Rough, ColorChannel.Green),
                    new ResourcePackRoughChannelProperties(TextureTags.Rough, ColorChannel.Blue),
                },
                [TextureTags.Metal] = profile => new ResourcePackChannelProperties[] {
                    new ResourcePackMetalChannelProperties(TextureTags.Metal, ColorChannel.Red),
                    new ResourcePackMetalChannelProperties(TextureTags.Metal, ColorChannel.Green),
                    new ResourcePackMetalChannelProperties(TextureTags.Metal, ColorChannel.Blue),
                },
                [TextureTags.F0] = profile => new ResourcePackChannelProperties[] {
                    new ResourcePackF0ChannelProperties(TextureTags.F0, ColorChannel.Red),
                    new ResourcePackF0ChannelProperties(TextureTags.F0, ColorChannel.Green),
                    new ResourcePackF0ChannelProperties(TextureTags.F0, ColorChannel.Blue),
                },
                [TextureTags.Porosity] = profile => new ResourcePackChannelProperties[] {
                    new ResourcePackPorosityChannelProperties(TextureTags.Porosity, ColorChannel.Red),
                    new ResourcePackPorosityChannelProperties(TextureTags.Porosity, ColorChannel.Green),
                    new ResourcePackPorosityChannelProperties(TextureTags.Porosity, ColorChannel.Blue),
                },
                [TextureTags.SubSurfaceScattering] = profile => new ResourcePackChannelProperties[] {
                    new ResourcePackSssChannelProperties(TextureTags.SubSurfaceScattering, ColorChannel.Red),
                    new ResourcePackSssChannelProperties(TextureTags.SubSurfaceScattering, ColorChannel.Green),
                    new ResourcePackSssChannelProperties(TextureTags.SubSurfaceScattering, ColorChannel.Blue),
                },
                [TextureTags.Emissive] = profile => new ResourcePackChannelProperties[] {
                    new ResourcePackEmissiveChannelProperties(TextureTags.Emissive, ColorChannel.Red),
                    new ResourcePackEmissiveChannelProperties(TextureTags.Emissive, ColorChannel.Green),
                    new ResourcePackEmissiveChannelProperties(TextureTags.Emissive, ColorChannel.Blue),
                },
            };
    }
}
