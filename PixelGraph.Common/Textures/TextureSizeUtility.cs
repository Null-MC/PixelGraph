using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using SixLabors.ImageSharp;
using System;

namespace PixelGraph.Common.Textures
{
    internal static class TextureSizeUtility
    {
        public static Size? GetSizeByType(PublishProfileProperties profile, MaterialType type, float? defaultAspect = null)
        {
            switch (type) {
                case MaterialType.Block:
                    if (TryGetBlockSize(profile, out var blockSize, defaultAspect))
                        return blockSize;
                    break;
                case MaterialType.Item:
                    if (TryGetItemSize(profile, out var itemSize, defaultAspect))
                        return itemSize;
                    break;
            }

            if (TryGetTextureSize(profile, out var texSize, defaultAspect))
                return texSize;

            return null;
        }

        public static bool TryGetBlockSize(PublishProfileProperties profile, out Size result, float? defaultAspect = null)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            return TryGetScaledSize(profile.BlockTextureSize, out result, defaultAspect);
        }

        public static bool TryGetItemSize(PublishProfileProperties profile, out Size result, float? defaultAspect = null)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            return TryGetScaledSize(profile.ItemTextureSize, out result, defaultAspect);
        }

        public static bool TryGetTextureSize(PublishProfileProperties profile, out Size result, float? defaultAspect = null)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            return TryGetScaledSize(profile.TextureSize, out result, defaultAspect);
        }

        private static bool TryGetScaledSize(in int? targetSize, out Size result, float? defaultAspect = null)
        {
            if (targetSize.HasValue) {
                result = AdjustAspect(targetSize.Value, defaultAspect);
                return true;
            }

            result = Size.Empty;
            return false;
        }

        //public static Size? GetItemSize(ResourcePackProfileProperties profile, float? defaultAspect = null)
        //{
        //    var targetSize = profile?.ItemTextureSize ?? profile?.TextureSize;
        //    return targetSize.HasValue ? AdjustAspect(targetSize.Value, defaultAspect) : null;
        //}

        private static Size AdjustAspect(in int targetWidth, in float? defaultAspect)
        {
            var aspect = defaultAspect ?? 1f;
            var scaledHeight = (int)MathF.Ceiling(targetWidth * aspect);
            return new Size(targetWidth, scaledHeight);
        }

        //public static MaterialType GetFinalMaterialType()
        //{
        //    var type = GetMaterialType();

        //    if (type == MaterialType.Automatic && !string.IsNullOrWhiteSpace(Material.LocalPath)) {
        //        var path = PathEx.Normalize(Material.LocalPath);

        //        if (path != null) {
        //            if (blockTextureExp.IsMatch(path)) return MaterialType.Block;
        //            if (itemTextureExp.IsMatch(path)) return MaterialType.Item;
        //            if (ctmTextureExp.IsMatch(path)) return MaterialType.Block;
        //            if (citTextureExp.IsMatch(path)) return MaterialType.Item;
        //            if (entityTextureExp.IsMatch(path)) return MaterialType.Entity;
        //        }
        //    }

        //    return type;
        //}

        //public MaterialType GetMaterialType()
        //{
        //    if (!string.IsNullOrWhiteSpace(Material?.Type)) {
        //        if (Enum.TryParse(typeof(MaterialType), Material.Type, out var type) && type != null)
        //            return (MaterialType) type;
        //    }
            
        //    return MaterialType.Automatic;
        //}
    }
}
