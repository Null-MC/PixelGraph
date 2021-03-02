using PixelGraph.Common.Encoding;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Samplers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PixelGraph.Common
{
    public class MaterialContext : ResourcePackContext, IDisposable
    {
        private static readonly Regex blockTextureExp = new Regex(@"(?:^|\/)textures\/block(?:\/|$)", RegexOptions.Compiled);
        private static readonly Regex entityTextureExp = new Regex(@"(?:^|\/)textures\/entity(?:\/|$)", RegexOptions.Compiled);

        public MaterialProperties Material {get; set;}

        public bool WrapX => Material.WrapX ?? MaterialProperties.DefaultWrap;
        public bool WrapY => Material.WrapY ?? MaterialProperties.DefaultWrap;
        public float? TextureScale => (float?)Profile?.TextureScale;
        public string DefaultSampler => Profile?.Encoding?.Sampler ?? Sampler.Nearest;
        public string ImageFormat => Profile?.Encoding?.Image ?? ResourcePackOutputProperties.ImageDefault;

        public bool AutoGenerateOcclusion => Profile?.AutoGenerateOcclusion ?? ResourcePackProfileProperties.AutoGenerateOcclusionDefault;


        public List<ResourcePackChannelProperties> InputEncoding {get; set;}
        public List<ResourcePackChannelProperties> OutputEncoding {get; set;}
        public Image<Rgb24> OcclusionTexture {get; set;}
        public Image<Rgb24> NormalTexture {get; set;}


        public MaterialContext()
        {
            InputEncoding = new List<ResourcePackChannelProperties>();
            OutputEncoding = new List<ResourcePackChannelProperties>();
        }

        public void Dispose()
        {
            NormalTexture?.Dispose();
            OcclusionTexture?.Dispose();
        }

        public void ApplyInputEncoding()
        {
            var inputFormat = TextureEncoding.GetFactory(Input.Format);
            var inputEncoding = inputFormat?.Create() ?? new ResourcePackEncoding();
            inputEncoding.Merge(Input);
            inputEncoding.Merge(Material);

            var outputFormat = TextureEncoding.GetFactory(Profile.Encoding.Format);
            var outputEncoding = outputFormat?.Create() ?? new ResourcePackEncoding();
            outputEncoding.Merge(Profile.Encoding);

            InputEncoding = inputEncoding.GetMapped().ToList();
            OutputEncoding = outputEncoding.GetMapped().ToList();
        }

        public void ApplyOutputEncoding()
        {
            var inputFormat = TextureEncoding.GetFactory(Profile.Encoding.Format);
            var inputEncoding = inputFormat?.Create() ?? new ResourcePackEncoding();
            inputEncoding.Merge(Profile.Encoding);

            var outputFormat = TextureEncoding.GetFactory(Input.Format);
            var outputEncoding = outputFormat?.Create() ?? new ResourcePackEncoding();
            outputEncoding.Merge(Input);
            // TODO: layer material properties on top of pack encoding?

            InputEncoding = inputEncoding.GetMapped().ToList();
            OutputEncoding = outputEncoding.GetMapped().ToList();
        }

        public MaterialType GetMaterialType()
        {
            if (!string.IsNullOrWhiteSpace(Material?.Type)) {
                if (Enum.TryParse(typeof(MaterialType), Material.Type, out var type) && type != null)
                    return (MaterialType) type;
            }
            
            return MaterialType.Automatic;
        }

        public MaterialType GetFinalMaterialType()
        {
            var type = GetMaterialType();

            if (type == MaterialType.Automatic) {
                var path = PathEx.ToUnixStyle(Material.LocalPath);

                if (path != null) {
                    if (blockTextureExp.IsMatch(path)) return MaterialType.Block;
                    if (entityTextureExp.IsMatch(path)) return MaterialType.Entity;
                }
            }

            return type;
        }

        public Size? GetTextureSize(float? defaultAspect)
        {
            if (Material.TextureWidth.HasValue) {
                if (Material.TextureHeight.HasValue) {
                    var width = Material.TextureWidth.Value;
                    var height = Material.TextureHeight.Value;

                    if (Profile.TextureScale.HasValue) {
                        var scale = (float)Profile.TextureScale.Value;
                        width = (int)MathF.Ceiling(width * scale);
                        height = (int)MathF.Ceiling(height * scale);
                    }

                    return new Size(width, height);
                }

                if (defaultAspect.HasValue) {
                    // TODO: return width, width*aspect
                }
            }
            else if (Material.TextureHeight.HasValue) {
                if (defaultAspect.HasValue) {
                    // TODO: return height/aspect, height
                }
            }

            if (Material.TextureSize.HasValue) {
                var aspect = defaultAspect ?? 1f;
                var width = Material.TextureSize.Value;
                var height = (int)MathF.Ceiling(width * aspect);

                if (Profile.TextureScale.HasValue) {
                    var scale = (float)Profile.TextureScale.Value;
                    width = (int)MathF.Ceiling(width * scale);
                    height = (int)MathF.Ceiling(height * scale);
                }

                return new Size(width, height);
            }

            var type = GetFinalMaterialType();

            switch (type) {
                case MaterialType.Block:
                    if (Profile?.BlockTextureSize.HasValue ?? false) {
                        var aspect = defaultAspect ?? 1f;
                        var width = Profile.BlockTextureSize.Value;
                        var height = (int)MathF.Ceiling(width * aspect);
                        return new Size(width, height);
                    }
                    break;
            }

            if (Profile?.TextureSize.HasValue ?? false) {
                var aspect = defaultAspect ?? 1f;
                var width = Profile.TextureSize.Value;
                var height = (int)MathF.Ceiling(width * aspect);
                return new Size(width, height);
            }

            return null;
        }

        public Size? GetBufferSize(float aspect)
        {
            if (Material.TryGetSourceBounds(out var bounds)) {
                if (TextureScale.HasValue) {
                    var width = (int)MathF.Ceiling(bounds.Width * TextureScale.Value);
                    var height = (int)MathF.Ceiling(bounds.Height * TextureScale.Value);
                    return new Size(width, height);
                }

                return bounds;
            }

            return GetTextureSize(aspect);
        }
    }
}
