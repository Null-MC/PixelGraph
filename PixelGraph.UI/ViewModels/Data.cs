using PixelGraph.Common.Encoding;
using PixelGraph.Common.Textures;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewModels
{
    internal class ProfileItem
    {
        public string Name {get; set;}
        public string Filename {get; set;}
    }

    internal class EncodingFormatValues : List<EncodingFormatValues.Format>
    {
        public EncodingFormatValues()
        {
            Add(new Format {Text = "None"});
            Add(new Format {Text = "Raw", Value = EncodingProperties.Raw, Hint = "All texture channel mappings."});
            Add(new Format {Text = "Default", Value = EncodingProperties.Default});
            Add(new Format {Text = "Legacy", Value = EncodingProperties.Legacy});
            Add(new Format {Text = "LAB 1.1", Value = EncodingProperties.Lab11});
            Add(new Format {Text = "LAB 1.3", Value = EncodingProperties.Lab13});
        }

        public class Format
        {
            public string Text {get; set;}
            public string Value {get; set;}
            public string Hint {get; set;}
        }
    }

    internal class TextureTagValues : List<TextureTagValues.Texture>
    {
        public TextureTagValues()
        {
            Add(new Texture {Text = "Albedo", Value = TextureTags.Albedo});
            Add(new Texture {Text = "Height", Value = TextureTags.Height});
            Add(new Texture {Text = "Normal", Value = TextureTags.Normal});
            Add(new Texture {Text = "Occlusion", Value = TextureTags.Occlusion});
            Add(new Texture {Text = "Specular", Value = TextureTags.Specular});
            Add(new Texture {Text = "Rough", Value = TextureTags.Rough});
            Add(new Texture {Text = "Smooth", Value = TextureTags.Smooth});
            Add(new Texture {Text = "Metal", Value = TextureTags.Metal});
            Add(new Texture {Text = "Porosity", Value = TextureTags.Porosity});
            Add(new Texture {Text = "SubSurface Scattering", Value = TextureTags.SubSurfaceScattering});
            Add(new Texture {Text = "Emissive", Value = TextureTags.Emissive});
        }

        public class Texture
        {
            public string Text {get; set;}
            public string Value {get; set;}
        }
    }

    internal class EncodingChannelValues : List<EncodingChannelValues.Channel>
    {
        public EncodingChannelValues()
        {
            Add(new Channel {Text = string.Empty});
            Add(new Channel {Text = "Red", Value = EncodingChannel.Red});
            Add(new Channel {Text = "Green", Value = EncodingChannel.Green});
            Add(new Channel {Text = "Blue", Value = EncodingChannel.Blue});
            Add(new Channel {Text = "Alpha", Value = EncodingChannel.Alpha});
            Add(new Channel {Text = "Height", Value = EncodingChannel.Height});
            Add(new Channel {Text = "Occlusion", Value = EncodingChannel.Occlusion});
            Add(new Channel {Text = "Normal-X", Value = EncodingChannel.NormalX});
            Add(new Channel {Text = "Normal-Y", Value = EncodingChannel.NormalY});
            Add(new Channel {Text = "Normal-Z", Value = EncodingChannel.NormalZ});
            Add(new Channel {Text = "Specular", Value = EncodingChannel.Specular});
            Add(new Channel {Text = "Smoothness", Value = EncodingChannel.Smooth});
            Add(new Channel {Text = "Perceptual Smoothness", Value = EncodingChannel.PerceptualSmooth});
            Add(new Channel {Text = "Roughness", Value = EncodingChannel.Rough});
            Add(new Channel {Text = "Metalness", Value = EncodingChannel.Metal});
            Add(new Channel {Text = "Porosity", Value = EncodingChannel.Porosity});
            Add(new Channel {Text = "Porosity + SSS", Value = EncodingChannel.Porosity_SSS});
            Add(new Channel {Text = "SSS", Value = EncodingChannel.SubSurfaceScattering});
            Add(new Channel {Text = "Emissive", Value = EncodingChannel.Emissive});
            Add(new Channel {Text = "Emissive Clipped", Value = EncodingChannel.EmissiveClipped});
            Add(new Channel {Text = "Emissive Inverse", Value = EncodingChannel.EmissiveInverse});
            Add(new Channel {Text = "White", Value = EncodingChannel.White});
        }

        public class Channel
        {
            public string Text {get; set;}
            public string Value {get; set;}
        }
    }
}
