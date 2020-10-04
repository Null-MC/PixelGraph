using McPbrPipeline.ImageProcessors;
using McPbrPipeline.Internal.Filtering;
using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Output;
using McPbrPipeline.Internal.Textures;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Publishing
{
    internal class SpecularTexturePublisher : TexturePublisherBase
    {
        public SpecularTexturePublisher(IProfile profile, IInputReader reader, IOutputWriter writer) : base(profile, reader, writer) {}

        public async Task PublishAsync(IPbrProperties texture, CancellationToken token)
        {
            var sourcePath = Profile.GetSourcePath(texture.Path);
            if (!texture.UseGlobalMatching) sourcePath = Path.Combine(sourcePath, texture.Name);
            var sourceFile = GetFilename(texture, TextureTags.Specular, sourcePath, texture.SpecularTexture);
            var destinationFile = Path.Combine(texture.Path, $"{texture.Name}_s.png");

            Rgba32? sourceColor = null;
            if (texture.SpecularColor != null)
                sourceColor = Rgba32.ParseHex(texture.SpecularColor);

            await PublishAsync(sourceFile, sourceColor, destinationFile, context => {
                ApplyScaleFilter(context, texture);

                //if (specularMap?.HasOffsets() ?? false)
                //    filters.Append(BuildRangeFilter(specularMap));

                Resize(context, texture);

                if (!Profile.SpecularChannelsMatch())
                    ApplyChannelMapFilter(context);
            }, token);

            //if (specularMap?.Metadata != null)
            //    await PublishMcMetaAsync(specularMap.Metadata, destinationFilename, token);
        }

        private void ApplyScaleFilter(IImageProcessingContext context, IPbrProperties texture)
        {
            var options = new ScaleOptions();

            if (Profile.SpecularIn.Smooth != ColorChannel.None)
                options.Set(Profile.SpecularIn.Smooth, texture.SmoothScale);

            if (Profile.SpecularIn.Rough != ColorChannel.None)
                options.Set(Profile.SpecularIn.Rough, texture.RoughScale);

            if (Profile.SpecularIn.Metal != ColorChannel.None)
                options.Set(Profile.SpecularIn.Metal, texture.MetalScale);

            if (Profile.SpecularIn.Emissive != ColorChannel.None)
                options.Set(Profile.SpecularIn.Emissive, texture.EmissiveScale);

            if (!options.Any) return;

            var processor = new ScaleProcessor(options);
            context.ApplyProcessor(processor);
        }

        //private RangeFilter BuildRangeFilter(IPbrProperties texture)
        //{
        //    var options = new RangeOptions();

        //    if (Profile.SpecularIn.Rough != ColorChannel.None) {
        //        if (specularMap.RoughMin.HasValue)
        //            options.SetMin(Profile.SpecularIn.Rough, specularMap.RoughMin.Value);

        //        if (specularMap.RoughMax.HasValue)
        //            options.SetMax(Profile.SpecularIn.Rough, specularMap.RoughMax.Value);
        //    }

        //    if (Profile.SpecularIn.Smooth != ColorChannel.None) {
        //        if (specularMap.SmoothMin.HasValue)
        //            options.SetMin(Profile.SpecularIn.Smooth, specularMap.SmoothMin.Value);

        //        if (specularMap.SmoothMax.HasValue)
        //            options.SetMax(Profile.SpecularIn.Smooth, specularMap.SmoothMax.Value);
        //    }

        //    if (Profile.SpecularIn.Metal != ColorChannel.None) {
        //        if (specularMap.MetalMin.HasValue)
        //            options.SetMin(Profile.SpecularIn.Metal, specularMap.MetalMin.Value);

        //        if (specularMap.MetalMax.HasValue)
        //            options.SetMax(Profile.SpecularIn.Metal, specularMap.MetalMax.Value);
        //    }

        //    if (Profile.SpecularIn.Emissive != ColorChannel.None) {
        //        if (specularMap.EmissiveMin.HasValue)
        //            options.SetMin(Profile.SpecularIn.Emissive, specularMap.EmissiveMin.Value);

        //        if (specularMap.EmissiveMax.HasValue)
        //            options.SetMax(Profile.SpecularIn.Emissive, specularMap.EmissiveMax.Value);
        //    }

        //    return new RangeFilter(options);
        //}

        private void ApplyChannelMapFilter(IImageProcessingContext context)
        {
            var options = new ChannelMapOptions {
                AlphaSource = ColorChannel.Alpha,
            };

            options.Set(Profile.SpecularIn.Rough, Profile.SpecularOut.Rough);
            options.Set(Profile.SpecularIn.Smooth, Profile.SpecularOut.Smooth);
            options.Set(Profile.SpecularIn.Metal, Profile.SpecularOut.Metal);
            options.Set(Profile.SpecularIn.Emissive, Profile.SpecularOut.Emissive);

            var processor = new ChannelMapProcessor(options);
            context.ApplyProcessor(processor);
        }
    }
}
