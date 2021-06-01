using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.TextureFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PixelGraph.Common.Textures
{
    public interface ITextureGraphContext
    {
        MaterialProperties Material {get; set;}
        ResourcePackInputProperties Input {get; set;}
        ResourcePackProfileProperties Profile {get; set;}
        List<ResourcePackChannelProperties> InputEncoding {get; set;}
        List<ResourcePackChannelProperties> OutputEncoding {get; set;}
        IPublisherMapping Mapping {get; set;}

        //string DestinationName {get; set;}
        //string DestinationPath {get; set;}
        bool IsAnimated {get; set;}
        int MaxFrameCount {get; set;}
        bool PublishAsGlobal {get; set;}
        bool IsImport {get; set;}
        bool MaterialWrapX {get;}
        bool MaterialWrapY {get;}
        bool IsMaterialMultiPart {get;}
        bool IsMaterialCtm {get;}
        float? TextureScale {get;}
        string DefaultSampler {get;}
        string ImageFormat {get;}
        //bool AutoMaterial {get;}
        bool AutoGenerateOcclusion {get;}

        void ApplyInputEncoding();
        void ApplyOutputEncoding();
        ISampler<T> CreateSampler<T>(Image<T> image, string name) where T : unmanaged, IPixel<T>;
        Size? GetTextureSize(float? defaultAspect);
        Size? GetBufferSize(float aspect);
        string GetMetaInputFilename();
    }

    internal class TextureGraphContext : ITextureGraphContext
    {
        private static readonly Regex blockTextureExp = new(@"(?:^|\/)textures\/block(?:\/|$)", RegexOptions.Compiled);
        private static readonly Regex ctmTextureExp = new(@"(?:^|\/)optifine\/ctm(?:\/|$)", RegexOptions.Compiled);
        private static readonly Regex entityTextureExp = new(@"(?:^|\/)textures\/entity(?:\/|$)", RegexOptions.Compiled);

        public MaterialProperties Material {get; set;}
        public ResourcePackInputProperties Input {get; set;}
        public ResourcePackProfileProperties Profile {get; set;}
        public List<ResourcePackChannelProperties> InputEncoding {get; set;}
        public List<ResourcePackChannelProperties> OutputEncoding {get; set;}
        public IPublisherMapping Mapping {get; set;}
        public string DestinationName {get; set;}
        public string DestinationPath {get; set;}
        public bool IsAnimated {get; set;}
        public int MaxFrameCount {get; set;}
        public bool PublishAsGlobal {get; set;}
        public bool IsImport {get; set;}

        public bool MaterialWrapX => Material.WrapX ?? MaterialProperties.DefaultWrap;
        public bool MaterialWrapY => Material.WrapY ?? MaterialProperties.DefaultWrap;
        public bool IsMaterialMultiPart => Material.Parts?.Any() ?? false;
        public bool IsMaterialCtm => !string.IsNullOrWhiteSpace(Material.CTM?.Type);
        public float? TextureScale => (float?)Profile?.TextureScale;
        public string DefaultSampler => Profile?.Encoding?.Sampler ?? Samplers.Samplers.Nearest;
        public string ImageFormat => Profile?.Encoding?.Image ?? ResourcePackOutputProperties.ImageDefault;

        //public bool AutoMaterial => Input.AutoMaterial ?? ResourcePackInputProperties.AutoMaterialDefault;
        public bool AutoGenerateOcclusion => Profile?.AutoGenerateOcclusion ?? ResourcePackProfileProperties.AutoGenerateOcclusionDefault;

        
        public TextureGraphContext()
        {
            InputEncoding = new List<ResourcePackChannelProperties>();
            OutputEncoding = new List<ResourcePackChannelProperties>();
            PublishAsGlobal = true;
            MaxFrameCount = 1;
        }

        public string GetMetaInputFilename()
        {
            var matPath = Material.UseGlobalMatching
                ? Material.LocalPath
                : PathEx.Join(Material.LocalPath, Material.Name);

            return PathEx.Join(matPath, "mat.mcmeta");
        }

        public void ApplyInputEncoding()
        {
            var inputFormat = TextureFormat.GetFactory(Input.Format);
            var inputEncoding = inputFormat?.Create() ?? new ResourcePackEncoding();
            inputEncoding.Merge(Input);
            inputEncoding.Merge(Material);

            var outputFormat = TextureFormat.GetFactory(Profile.Encoding.Format);
            var outputEncoding = outputFormat?.Create() ?? new ResourcePackEncoding();
            outputEncoding.Merge(Profile.Encoding);

            InputEncoding = inputEncoding.GetMapped().ToList();
            OutputEncoding = outputEncoding.GetMapped().ToList();
        }

        public void ApplyOutputEncoding()
        {
            var inputFormat = TextureFormat.GetFactory(Profile.Encoding.Format);
            var inputEncoding = inputFormat?.Create() ?? new ResourcePackEncoding();
            inputEncoding.Merge(Profile.Encoding);

            var outputFormat = TextureFormat.GetFactory(Input.Format);
            var outputEncoding = outputFormat?.Create() ?? new ResourcePackEncoding();
            outputEncoding.Merge(Input);
            // TODO: layer material properties on top of pack encoding?

            InputEncoding = inputEncoding.GetMapped().ToList();
            if (InputEncoding.Count == 0) throw new ApplicationException("Input encoding is empty!");

            OutputEncoding = outputEncoding.GetMapped().ToList();
            if (OutputEncoding.Count == 0) throw new ApplicationException("Output encoding is empty!");
        }

        public ISampler<T> CreateSampler<T>(Image<T> image, string name) where T : unmanaged, IPixel<T>
        {
            var sampler = Sampler<T>.Create(name);
            sampler.Image = image;
            sampler.WrapX = MaterialWrapX;
            sampler.WrapY = MaterialWrapY;

            return sampler;
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
                var path = PathEx.Normalize(Material.LocalPath);

                if (path != null) {
                    if (blockTextureExp.IsMatch(path)) return MaterialType.Block;
                    if (ctmTextureExp.IsMatch(path)) return MaterialType.Block;
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

                if (Profile?.TextureScale.HasValue ?? false) {
                    var scale = (float)Profile.TextureScale.Value;
                    width = (int)MathF.Ceiling(width * scale);
                    height = (int)MathF.Ceiling(height * scale);
                }

                return new Size(width, height);
            }

            var type = GetFinalMaterialType();

            switch (type) {
                case MaterialType.Block:
                    var targetSize = Profile?.BlockTextureSize ?? Profile?.TextureSize;

                    if (targetSize.HasValue) {
                        var aspect = defaultAspect ?? 1f;
                        var width = targetSize.Value;
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
            var blockSize = Profile?.BlockTextureSize;
            var scale = (float?)Profile?.TextureScale;

            if (!Material.TryGetSourceBounds(in blockSize, scale, out var bounds))
                return GetTextureSize(aspect);

            return bounds;
            //if (!TextureScale.HasValue) return bounds;

            //var width = (int)MathF.Ceiling(bounds.Width * TextureScale.Value);
            //var height = (int)MathF.Ceiling(bounds.Height * TextureScale.Value);
            //return new Size(width, height);
        }
    }
}
