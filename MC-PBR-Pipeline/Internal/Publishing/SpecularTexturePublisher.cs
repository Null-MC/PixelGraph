using McPbrPipeline.Filters;
using McPbrPipeline.Internal.Filtering;
using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Output;
using McPbrPipeline.Internal.Textures;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Publishing
{
    internal class SpecularTexturePublisher : TexturePublisherBase
    {
        public SpecularTexturePublisher(IProfile profile, IInputReader reader, IOutputWriter writer) : base(profile, reader, writer) {}

        public async Task PublishAsync(TextureCollection texture, CancellationToken token)
        {
            var sourcePath = Profile.GetSourcePath(texture.Path);
            if (!texture.UseGlobalMatching) sourcePath = Path.Combine(sourcePath, texture.Name);
            var destinationFilename = Path.Combine(texture.Path, $"{texture.Name}_s.png");
            var specularMap = texture.Map.Specular;

            var filters = new FilterChain(Reader, Writer) {
                DestinationFilename = destinationFilename,
                SourceFilename = GetFilename(texture, TextureTags.Specular, sourcePath, specularMap?.Texture),
            };

            if (specularMap?.Color != null)
                filters.SourceColor = Rgba32.ParseHex(specularMap.Color);

            if (specularMap?.HasScaling() ?? false)
                filters.Append(BuildScaleFilter(specularMap));

            if (specularMap?.HasOffsets() ?? false)
                filters.Append(BuildRangeFilter(specularMap));

            Resize(filters, texture);

            if (!Profile.SpecularChannelsMatch())
                filters.Append(BuildChannelMapFilter());

            await filters.ApplyAsync(token);

            if (specularMap?.Metadata != null)
                await PublishMcMetaAsync(specularMap.Metadata, destinationFilename, token);
        }

        private ScaleFilter BuildScaleFilter(SpecularTextureMap specularMap)
        {
            var options = new ScaleOptions();

            if (Profile.SpecularIn.Rough != ColorChannel.None && specularMap.RoughScale.HasValue)
                options.Set(Profile.SpecularIn.Rough, specularMap.RoughScale.Value);

            if (Profile.SpecularIn.Smooth != ColorChannel.None && specularMap.SmoothScale.HasValue)
                options.Set(Profile.SpecularIn.Smooth, specularMap.SmoothScale.Value);

            if (Profile.SpecularIn.Metal != ColorChannel.None && specularMap.MetalScale.HasValue)
                options.Set(Profile.SpecularIn.Metal, specularMap.MetalScale.Value);

            if (Profile.SpecularIn.Emissive != ColorChannel.None && specularMap.EmissiveScale.HasValue)
                options.Set(Profile.SpecularIn.Emissive, specularMap.EmissiveScale.Value);

            return new ScaleFilter(options);
        }

        private RangeFilter BuildRangeFilter(SpecularTextureMap specularMap)
        {
            var options = new RangeOptions();

            if (Profile.SpecularIn.Rough != ColorChannel.None) {
                if (specularMap.RoughMin.HasValue)
                    options.SetMin(Profile.SpecularIn.Rough, specularMap.RoughMin.Value);

                if (specularMap.RoughMax.HasValue)
                    options.SetMax(Profile.SpecularIn.Rough, specularMap.RoughMax.Value);
            }

            if (Profile.SpecularIn.Smooth != ColorChannel.None) {
                if (specularMap.SmoothMin.HasValue)
                    options.SetMin(Profile.SpecularIn.Smooth, specularMap.SmoothMin.Value);

                if (specularMap.SmoothMax.HasValue)
                    options.SetMax(Profile.SpecularIn.Smooth, specularMap.SmoothMax.Value);
            }

            if (Profile.SpecularIn.Metal != ColorChannel.None) {
                if (specularMap.MetalMin.HasValue)
                    options.SetMin(Profile.SpecularIn.Metal, specularMap.MetalMin.Value);

                if (specularMap.MetalMax.HasValue)
                    options.SetMax(Profile.SpecularIn.Metal, specularMap.MetalMax.Value);
            }

            if (Profile.SpecularIn.Emissive != ColorChannel.None) {
                if (specularMap.EmissiveMin.HasValue)
                    options.SetMin(Profile.SpecularIn.Emissive, specularMap.EmissiveMin.Value);

                if (specularMap.EmissiveMax.HasValue)
                    options.SetMax(Profile.SpecularIn.Emissive, specularMap.EmissiveMax.Value);
            }

            return new RangeFilter(options);
        }

        private ChannelMapFilter BuildChannelMapFilter()
        {
            var options = new ChannelMapOptions {
                AlphaSource = ColorChannel.Alpha,
            };

            options.Set(Profile.SpecularIn.Rough, Profile.SpecularOut.Rough);
            options.Set(Profile.SpecularIn.Smooth, Profile.SpecularOut.Smooth);
            options.Set(Profile.SpecularIn.Metal, Profile.SpecularOut.Metal);
            options.Set(Profile.SpecularIn.Emissive, Profile.SpecularOut.Emissive);

            return new ChannelMapFilter(options);
        }
    }
}
