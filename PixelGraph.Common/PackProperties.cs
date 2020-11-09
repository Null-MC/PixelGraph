using PixelGraph.Common.IO;
using PixelGraph.Common.Textures;
using System;

namespace PixelGraph.Common
{
    public class PackProperties : PropertiesFile
    {
        public string Source {get; set;}
        public DateTime WriteTime {get; set;}

        public string PackEdition {
            get => Get("pack.edition", "Java");
            set => Properties["pack.edition"] = value;
        }

        public int PackFormat {
            get => Get("pack.format", 5);
            set => Properties["pack.format"] = value.ToString();
        }

        public string PackDescription {
            get => Get<string>("pack.description");
            set => Properties["pack.description"] = value;
        }

        public string PackTags {
            get => Get<string>("pack.tags");
            set => Properties["pack.tags"] = value;
        }

        public string Sampler => Get<string>("sampler");
        public int? TextureSize => Get<int?>("texture.size");
        public float? TextureScale => Get<float?>("texture.scale");

        public string InputFormat {
            get => Get<string>("input.format");
            set => Properties["input.format"] = value;
        }

        public string OutputFormat {
            get => Get<string>("output.format");
            set => Properties["output.format"] = value;
        }

        public string OutputEncoding => Get("output.encoding", ImageExtensions.Default);


        public string GetInput(string textureTag, ColorChannel colorChannel)
        {
            var texture = GetTexturePart(textureTag);
            var channel = GetChannelPart(colorChannel);
            return Get<string>($"{texture}.input.{channel}");
        }

        public void SetInput(string textureTag, ColorChannel colorChannel, string value)
        {
            var texture = GetTexturePart(textureTag);
            var channel = GetChannelPart(colorChannel);
            Properties[$"{texture}.input.{channel}"] = value;
        }

        public string GetOutput(string textureTag, ColorChannel colorChannel)
        {
            var texture = GetTexturePart(textureTag);
            var channel = GetChannelPart(colorChannel);
            return Get<string>($"output.{texture}.{channel}");
        }

        public void SetOutput(string textureTag, ColorChannel colorChannel, string value)
        {
            var texture = GetTexturePart(textureTag);
            var channel = GetChannelPart(colorChannel);
            Properties[$"output.{texture}.{channel}"] = value;
        }

        public bool GetExported(string textureTag)
        {
            var texture = GetTexturePart(textureTag);
            return Get($"output.{texture}", false);
        }

        public void SetExported(string textureTag, bool value)
        {
            var texture = GetTexturePart(textureTag);
            Properties[$"output.{texture}"] = value.ToString();
        }

        // TODO: DUPLICATE CODE
        private static string GetTexturePart(string textureTag)
        {
            return textureTag switch {
                TextureTags.Albedo => "albedo",
                TextureTags.Height => "height",
                TextureTags.Normal => "normal",
                TextureTags.Occlusion => "occlusion",
                TextureTags.Specular => "specular",
                TextureTags.Rough => "rough",
                TextureTags.Smooth => "smooth",
                TextureTags.Metal => "metal",
                TextureTags.Porosity => "porosity",
                TextureTags.SubSurfaceScattering => "sss",
                TextureTags.Emissive => "emissive",
                // TODO: ...
                _ => null,
            };
        }

        // TODO: DUPLICATE CODE
        private static string GetChannelPart(ColorChannel colorChannel)
        {
            return colorChannel switch {
                ColorChannel.Red => "r",
                ColorChannel.Green => "g",
                ColorChannel.Blue => "b",
                ColorChannel.Alpha => "a",
                _ => null,
            };
        }
    }
}
