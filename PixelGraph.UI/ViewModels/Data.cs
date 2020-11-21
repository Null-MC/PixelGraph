using PixelGraph.Common.Encoding;
using PixelGraph.Common.Textures;
using System.Collections.Generic;

namespace PixelGraph.UI.ViewModels
{
    internal class ProfileItem
    {
        public string Name {get; set;}
        public string LocalFile {get; set;}
    }

    internal class EncodingFormatValues : List<EncodingFormatValues.Item>
    {
        public EncodingFormatValues()
        {
            Add(new Item {Text = "None"});
            Add(new Item {Text = "Raw", Value = TextureEncoding.Format_Raw, Hint = "All texture channel mappings."});
            Add(new Item {Text = "Default", Value = TextureEncoding.Format_Default});
            Add(new Item {Text = "Legacy", Value = TextureEncoding.Format_Legacy});
            Add(new Item {Text = "LAB 1.1", Value = TextureEncoding.Format_Lab11});
            Add(new Item {Text = "LAB 1.3", Value = TextureEncoding.Format_Lab13});
        }

        public class Item
        {
            public string Text {get; set;}
            public string Value {get; set;}
            public string Hint {get; set;}
        }
    }

    internal class TextureTagValues : List<TextureTagValues.Item>
    {
        public TextureTagValues()
        {
            Add(new Item {Text = "Albedo", Value = TextureTags.Albedo});
            Add(new Item {Text = "Height", Value = TextureTags.Height});
            Add(new Item {Text = "Normal", Value = TextureTags.Normal});
            Add(new Item {Text = "Occlusion", Value = TextureTags.Occlusion});
            Add(new Item {Text = "Specular", Value = TextureTags.Specular});
            Add(new Item {Text = "Rough", Value = TextureTags.Rough});
            Add(new Item {Text = "Smooth", Value = TextureTags.Smooth});
            Add(new Item {Text = "Metal", Value = TextureTags.Metal});
            Add(new Item {Text = "Porosity", Value = TextureTags.Porosity});
            Add(new Item {Text = "SubSurface Scattering", Value = TextureTags.SubSurfaceScattering});
            Add(new Item {Text = "Emissive", Value = TextureTags.Emissive});
        }

        public class Item
        {
            public string Text {get; set;}
            public string Value {get; set;}
        }
    }

    internal class OptionalTextureTagValues : List<OptionalTextureTagValues.Item>
    {
        public OptionalTextureTagValues()
        {
            Add(new Item {Text = string.Empty});
            Add(new Item {Text = "Albedo", Value = TextureTags.Albedo});
            Add(new Item {Text = "Height", Value = TextureTags.Height});
            Add(new Item {Text = "Normal", Value = TextureTags.Normal});
            Add(new Item {Text = "Occlusion", Value = TextureTags.Occlusion});
            Add(new Item {Text = "Specular", Value = TextureTags.Specular});
            Add(new Item {Text = "Rough", Value = TextureTags.Rough});
            Add(new Item {Text = "Smooth", Value = TextureTags.Smooth});
            Add(new Item {Text = "Metal", Value = TextureTags.Metal});
            Add(new Item {Text = "Porosity", Value = TextureTags.Porosity});
            Add(new Item {Text = "SubSurface Scattering", Value = TextureTags.SubSurfaceScattering});
            Add(new Item {Text = "Emissive", Value = TextureTags.Emissive});
        }

        public class Item
        {
            public string Text {get; set;}
            public string Value {get; set;}
        }
    }

    internal class ColorChannelValues : List<ColorChannelValues.Item>
    {
        public ColorChannelValues()
        {
            Add(new Item {Text = string.Empty});
            Add(new Item {Text = "Red", Value = ColorChannel.Red});
            Add(new Item {Text = "Green", Value = ColorChannel.Green});
            Add(new Item {Text = "Blue", Value = ColorChannel.Blue});
            Add(new Item {Text = "Alpha", Value = ColorChannel.Alpha});
        }

        public class Item
        {
            public string Text {get; set;}
            public ColorChannel? Value {get; set;}
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
