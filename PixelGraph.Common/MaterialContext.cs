using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Samplers;
using SixLabors.ImageSharp;
using System;
using System.Text.RegularExpressions;

namespace PixelGraph.Common
{
    public class MaterialContext : ResourcePackContext
    {
        private static readonly Regex blockTextureExp = new Regex(@"(?:^|\/)textures\/block(?:\/|$)", RegexOptions.Compiled);
        private static readonly Regex entityTextureExp = new Regex(@"(?:^|\/)textures\/entity(?:\/|$)", RegexOptions.Compiled);

        public MaterialProperties Material {get; set;}
        public bool CreateEmpty {get; set;}

        public bool WrapX => Material.WrapX ?? MaterialProperties.DefaultWrap;
        public bool WrapY => Material.WrapY ?? MaterialProperties.DefaultWrap;
        public float? TextureScale => Profile?.TextureScale;
        public string DefaultSampler => Profile?.Encoding?.Sampler ?? Sampler.Nearest;
        public bool AutoGenerateOcclusion => Profile?.AutoGenerateOcclusion ?? ResourcePackProfileProperties.AutoGenerateOcclusionDefault;


        public MaterialContext()
        {
            CreateEmpty = true;
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
                        width = (int)MathF.Ceiling(width * Profile.TextureScale.Value);
                        height = (int)MathF.Ceiling(height * Profile.TextureScale.Value);
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

            //var width = Material.TextureWidth ?? Material.TextureSize;
            //var height = Material.TextureHeight ?? Material.TextureSize;

            if (Material.TextureSize.HasValue) {
                var aspect = defaultAspect ?? 1f;
                var width = Material.TextureSize.Value;
                var height = (int)MathF.Ceiling(width * aspect);

                if (Profile.TextureScale.HasValue) {
                    width = (int)MathF.Ceiling(width * Profile.TextureScale.Value);
                    height = (int)MathF.Ceiling(height * Profile.TextureScale.Value);
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
