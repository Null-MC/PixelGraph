﻿using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.TextureFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace PixelGraph.Common.Textures.Graphing
{
    public interface ITextureGraphContext
    {
        MaterialProperties Material {get; set;}
        ResourcePackInputProperties Input {get; set;}
        ResourcePackProfileProperties Profile {get; set;}
        TextureMappingCollection InputEncoding {get; set;}
        TextureMappingCollection OutputEncoding {get; set;}
        IPublisherMapping Mapping {get; set;}

        //string DestinationName {get; set;}
        //string DestinationPath {get; set;}
        bool IsAnimated {get; set;}
        int MaxFrameCount {get; set;}
        bool PublishAsGlobal {get; set;}
        bool ApplyPostProcessing {get; set;}
        bool IsImport {get; set;}
        bool MaterialWrapX {get;}
        bool MaterialWrapY {get;}
        bool IsMaterialMultiPart {get;}
        bool IsMaterialCtm {get;}
        float? TextureScale {get;}
        string DefaultSampler {get;}
        //string ImageFormat {get;}
        //bool AutoMaterial {get;}
        bool AutoGenerateOcclusion {get;}
        bool BakeOcclusionToColor {get;}
        bool EnablePalette {get;}
        int PaletteColors {get;}

        void ApplyInputEncoding();
        void ApplyOutputEncoding();
        ISampler<T> CreateSampler<T>(Image<T> image, string name) where T : unmanaged, IPixel<T>;
        Size? GetTextureSize(float? defaultAspect);
        Size? GetBufferSize(float aspect);
        //string GetMetaInputFilename();
    }

    internal class TextureGraphContext : ITextureGraphContext
    {
        private static readonly Regex blockTextureExp = new(@"(?:^|\/)textures\/block(?:\/|$)", RegexOptions.Compiled);
        private static readonly Regex itemTextureExp = new(@"(?:^|\/)textures\/item(?:\/|$)", RegexOptions.Compiled);
        private static readonly Regex ctmTextureExp = new(@"(?:^|\/)optifine\/ctm(?:\/|$)", RegexOptions.Compiled);
        private static readonly Regex citTextureExp = new(@"(?:^|\/)optifine\/cit(?:\/|$)", RegexOptions.Compiled);
        private static readonly Regex entityTextureExp = new(@"(?:^|\/)textures\/entity(?:\/|$)", RegexOptions.Compiled);

        public MaterialProperties Material {get; set;}
        public ResourcePackInputProperties Input {get; set;}
        public ResourcePackProfileProperties Profile {get; set;}
        public TextureMappingCollection InputEncoding {get; set;}
        public TextureMappingCollection OutputEncoding {get; set;}
        public IPublisherMapping Mapping {get; set;}
        public bool IsAnimated {get; set;}
        public int MaxFrameCount {get; set;}
        public bool PublishAsGlobal {get; set;}
        public bool ApplyPostProcessing {get; set;}
        public bool IsImport {get; set;}

        public bool MaterialWrapX => Material.WrapX ?? MaterialProperties.DefaultWrap;
        public bool MaterialWrapY => Material.WrapY ?? MaterialProperties.DefaultWrap;
        public bool IsMaterialMultiPart => Material.Parts?.Any() ?? false;
        public bool IsMaterialCtm => !string.IsNullOrWhiteSpace(Material.CTM?.Method);

        public float? TextureScale => (float?)Profile?.TextureScale;
        public string DefaultSampler => Profile?.Encoding?.Sampler ?? Samplers.Samplers.Nearest;
        //public string ImageFormat => Profile?.Encoding?.Image ?? ResourcePackOutputProperties.ImageDefault;

        //public bool AutoMaterial => Input.AutoMaterial ?? ResourcePackInputProperties.AutoMaterialDefault;
        public bool AutoGenerateOcclusion => Profile?.AutoGenerateOcclusion ?? ResourcePackProfileProperties.AutoGenerateOcclusionDefault;

        public bool BakeOcclusionToColor {
            get {
                var matVal = Material.Color.BakeOcclusion;
                var profileVal = Profile?.BakeOcclusionToColor;

                if (!matVal.HasValue && !profileVal.HasValue)
                    return ResourcePackProfileProperties.BakeOcclusionToColorDefault;

                return (matVal ?? false) || (profileVal ?? false);
            }
        }

        //public bool EnablePalette => Material.EnablePalette ?? Profile?.EnablePalette ?? false;
        public bool EnablePalette => Profile?.Encoding?.EnablePalette ?? false;
        public int PaletteColors => Profile?.Encoding?.PaletteColors ?? ResourcePackOutputProperties.DefaultPaletteColors;


        public TextureGraphContext()
        {
            //InputEncoding = new List<ResourcePackChannelProperties>();
            //OutputEncoding = new List<ResourcePackChannelProperties>();
            ApplyPostProcessing = true;
            PublishAsGlobal = true;
            MaxFrameCount = 1;
        }

        //public string GetMetaInputFilename()
        //{
        //    var matPath = Material.UseGlobalMatching
        //        ? Material.LocalPath
        //        : PathEx.Join(Material.LocalPath, Material.Name);



        //    return PathEx.Join(matPath, "mat.mcmeta");
        //}

        public void ApplyInputEncoding()
        {
            InputEncoding = BuildEncoding(Input.Format);
            InputEncoding.Merge(Input);
            InputEncoding.Merge(Material);

            OutputEncoding = BuildEncoding(Profile.Encoding.Format);
            OutputEncoding.Merge(Profile.Encoding);

            //InputEncoding = inputEncoding.GetTexturesWithMappings().ToList();
            //OutputEncoding = outputEncoding.GetTexturesWithMappings().ToList();
        }

        public void ApplyOutputEncoding()
        {
            InputEncoding = BuildEncoding(Profile.Encoding.Format);
            InputEncoding.Merge(Profile.Encoding);
            if (!InputEncoding.GetTexturesWithMappings().Any()) throw new ApplicationException("Input encoding is empty!");

            OutputEncoding = BuildEncoding(Input.Format);
            OutputEncoding.Merge(Input);
            // TODO: layer material properties on top of pack encoding?
            if (!OutputEncoding.GetTexturesWithMappings().Any()) throw new ApplicationException("Output encoding is empty!");
        }

        public ISampler<T> CreateSampler<T>(Image<T> image, string name) where T : unmanaged, IPixel<T>
        {
            var sampler = Sampler<T>.Create(name);
            sampler.Image = image;
            sampler.WrapX = MaterialWrapX;
            sampler.WrapY = MaterialWrapY;

            return sampler;
        }

        public MaterialType GetFinalMaterialType()
        {
            var type = Material.GetMaterialType();

            if (type != MaterialType.Automatic || string.IsNullOrWhiteSpace(Material.LocalPath))
                return type;

            var path = PathEx.Normalize(Material.LocalPath);
            if (path == null) return type;

            if (blockTextureExp.IsMatch(path)) return MaterialType.Block;
            if (itemTextureExp.IsMatch(path)) return MaterialType.Item;
            if (ctmTextureExp.IsMatch(path)) return MaterialType.Block;
            if (citTextureExp.IsMatch(path)) return MaterialType.Item;
            if (entityTextureExp.IsMatch(path)) return MaterialType.Entity;

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

                if (Profile?.TextureScale.HasValue ?? false) {
                    var scale = (float)Profile.TextureScale.Value;
                    width = (int)MathF.Ceiling(width * scale);
                    height = (int)MathF.Ceiling(height * scale);
                }

                return new Size(width, height);
            }

            var type = GetFinalMaterialType();
            if (Profile != null)
                return TextureSizeUtility.GetSizeByType(Profile, type, defaultAspect);

            return null;
        }

        public Size? GetBufferSize(float aspect)
        {
            if (Profile == null) return null;

            var blockSize = Profile?.BlockTextureSize;
            var scale = (float?)Profile?.TextureScale;

            if (Material.TryGetSourceBounds(in blockSize, scale, out var bounds)) return bounds;

            return GetTextureSize(aspect);

            //if (!TextureScale.HasValue) return bounds;

            //var width = (int)MathF.Ceiling(bounds.Width * TextureScale.Value);
            //var height = (int)MathF.Ceiling(bounds.Height * TextureScale.Value);
            //return new Size(width, height);
        }

        private static TextureMappingCollection BuildEncoding(string format)
        {
            TextureMappingCollection encoding = null;
            if (!string.IsNullOrWhiteSpace(format)) {
                var formatFactory = TextureFormat.GetFactory(format);
                encoding = formatFactory?.Create();
            }

            return encoding ?? new TextureMappingCollection();
        }
    }
}
