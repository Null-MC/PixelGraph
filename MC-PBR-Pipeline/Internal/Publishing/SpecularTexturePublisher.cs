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
        public SpecularTexturePublisher(PackProperties pack, IInputReader reader, IOutputWriter writer) : base(pack, reader, writer) {}

        public async Task PublishAsync(PbrProperties texture, CancellationToken token)
        {
            var sourceFile = texture.GetTextureFile(Reader, TextureTags.Specular);
            var destinationFile = Path.Combine(texture.Path, $"{texture.Name}_s.png");

            Rgba32? sourceColor = null;
            if (texture.SpecularColor != null)
                sourceColor = Rgba32.ParseHex(texture.SpecularColor);

            await PublishAsync(sourceFile, sourceColor, destinationFile, context => {
                ApplyScaleFilter(context, texture);

                //if (specularMap?.HasOffsets() ?? false)
                //    filters.Append(BuildRangeFilter(specularMap));

                Resize(context, texture);

                if (!Pack.SpecularChannelsMatch())
                    ApplyChannelMapFilter(context);
            }, token);

            //if (specularMap?.Metadata != null)
            //    await PublishMcMetaAsync(specularMap.Metadata, destinationFilename, token);
        }

        private void ApplyScaleFilter(IImageProcessingContext context, PbrProperties texture)
        {
            var options = new ScaleOptions();

            if (Pack.SpecularIn.Smooth != ColorChannel.None)
                options.Set(Pack.SpecularIn.Smooth, texture.SmoothScale);

            if (Pack.SpecularIn.Rough != ColorChannel.None)
                options.Set(Pack.SpecularIn.Rough, texture.RoughScale);

            if (Pack.SpecularIn.Metal != ColorChannel.None)
                options.Set(Pack.SpecularIn.Metal, texture.MetalScale);

            if (Pack.SpecularIn.Emissive != ColorChannel.None)
                options.Set(Pack.SpecularIn.Emissive, texture.EmissiveScale);

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

            options.Set(Pack.SpecularIn.Rough, Pack.SpecularOut.Rough);
            options.Set(Pack.SpecularIn.Smooth, Pack.SpecularOut.Smooth);
            options.Set(Pack.SpecularIn.Metal, Pack.SpecularOut.Metal);
            options.Set(Pack.SpecularIn.Emissive, Pack.SpecularOut.Emissive);

            var processor = new ChannelMapProcessor(options);
            context.ApplyProcessor(processor);
        }
    }
}
