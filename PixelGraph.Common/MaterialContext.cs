using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Samplers;
using SixLabors.ImageSharp;
using System;
using System.Text.RegularExpressions;
using PixelGraph.Common.Extensions;

namespace PixelGraph.Common
{
    public class MaterialContext : ResourcePackContext
    {
        private static readonly Regex blockTextureExp = new Regex(@"(?:^|\/)textures\/block(?:\/|$)", RegexOptions.Compiled);
        private static readonly Regex entityTextureExp = new Regex(@"(?:^|\/)textures\/entity(?:\/|$)", RegexOptions.Compiled);

        public MaterialProperties Material {get; set;}
        //public MaterialType Type {get; set;}
        public bool CreateEmpty {get; set;}
        //public int DefaultSize {get; set;}
        //public Size BufferSize {get; set;}
        //public bool AutoGenerateOcclusion {get; set;}

        public bool WrapX => Material.WrapX ?? MaterialProperties.DefaultWrap;
        public bool WrapY => Material.WrapY ?? MaterialProperties.DefaultWrap;
        public int DefaultTextureSize => Profile?.DefaultTextureSize ?? 1;
        public string DefaultSampler => Profile?.Encoding?.Sampler ?? Sampler.Nearest;
        public bool AutoGenerateOcclusion => Profile?.AutoGenerateOcclusion ?? ResourcePackProfileProperties.AutoGenerateOcclusionDefault;


        public MaterialContext()
        {
            //DefaultSize = 1;
            CreateEmpty = true;
        }

        //public Size GetDefaultTextureSize()
        //{
        //    if (Material.TryGetSourceBounds(out var size)) return size;

        //    var length = Profile?.DefaultTextureSize ?? 1;
        //    return new Size(length, length);
        //}

        public MaterialType GetMaterialType()
        {
            if (!string.IsNullOrWhiteSpace(Material?.Type)) {
                if (Enum.TryParse(typeof(MaterialType), Material.Type, out var type) && type != null)
                    return (MaterialType) type;
            }
            
            return MaterialType.Automatic;
        }

        public MaterialType GetMaterialFinalType()
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

        public int? GetBufferSize()
        {
            return GetMaterialFinalType() switch {
                MaterialType.Block => Profile?.BlockTextureSize,
                MaterialType.Entity => Profile.EntityTextureSize,
                _ => null,
            };
        }

        public int GetFinalBufferSize()
        {
            return GetBufferSize() ?? DefaultTextureSize;
        }

        public Size GetTargetTextureSize()
        {
            if (Material.TryGetSourceBounds(out var size)) return size;

            var length = Profile?.DefaultTextureSize ?? 1;
            return new Size(length, length);
        }

        public bool TryGetScale(out float scale)
        {
            scale = Profile?.TextureScale ?? 0f;
            return Profile?.TextureScale.HasValue ?? false;
        }

        public void ApplyTargetTextureScale(ref Size size)
        {
            if (!(Profile?.TextureScale.HasValue ?? false)) return;

            size.Width = (int) MathF.Ceiling(size.Width * Profile.TextureScale.Value);
            size.Height = (int) MathF.Ceiling(size.Height * Profile.TextureScale.Value);
        }
    }
}
