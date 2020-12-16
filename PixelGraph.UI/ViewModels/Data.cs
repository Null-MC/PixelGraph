using PixelGraph.Common.Encoding;
using PixelGraph.Common.Samplers;
using PixelGraph.Common.Textures;
using System.Collections.Generic;
using System.Windows.Media;

namespace PixelGraph.UI.ViewModels
{
    internal class ProfileItem
    {
        public string Name {get; set;}
        public string LocalFile {get; set;}
    }

    public class LogMessageItem
    {
        public string Message {get; set;}
        public Brush Color {get; set;}
    }

    internal interface ISearchParameters
    {
        string SearchText {get;}
        bool ShowAllFiles {get;}
    }

    internal class EncodingFormatValues : List<EncodingFormatValues.Item>
    {
        public EncodingFormatValues()
        {
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

    internal class OptionalEncodingFormatValues : List<OptionalEncodingFormatValues.Item>
    {
        public OptionalEncodingFormatValues()
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
            Add(new Item {Text = "Alpha", Value = TextureTags.Alpha});
            Add(new Item {Text = "Diffuse", Value = TextureTags.Diffuse});
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
            Add(new Item {Text = "None", Value = TextureTags.None});
            //Add(new Item {Text = "White", Value = TextureTags.White});
            Add(new Item {Text = "Alpha", Value = TextureTags.Alpha});
            Add(new Item {Text = "Diffuse", Value = TextureTags.Diffuse});
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

    internal class OptionalColorChannelValues : List<OptionalColorChannelValues.Item>
    {
        public OptionalColorChannelValues()
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

    internal class SamplerValues : List<SamplerValues.Item>
    {
        public SamplerValues()
        {
            //Add(new Item {Text = "Point", Value = Sampler.Point});
            Add(new Item {Text = "Nearest", Value = Sampler.Nearest});
            Add(new Item {Text = "Bilinear", Value = Sampler.Bilinear});
            //Add(new Item {Text = "Cubic", Value = Sampler.Cubic});
            //Add(new Item {Text = "Bicubic", Value = Sampler.Bicubic});
        }

        public class Item
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
            Add(new Channel {Text = "Alpha", Value = EncodingChannel.Alpha});
            Add(new Channel {Text = "Diffuse Red", Value = EncodingChannel.DiffuseRed});
            Add(new Channel {Text = "Diffuse Green", Value = EncodingChannel.DiffuseGreen});
            Add(new Channel {Text = "Diffuse Blue", Value = EncodingChannel.DiffuseBlue});
            Add(new Channel {Text = "Albedo Red", Value = EncodingChannel.AlbedoRed});
            Add(new Channel {Text = "Albedo Green", Value = EncodingChannel.AlbedoGreen});
            Add(new Channel {Text = "Albedo Blue", Value = EncodingChannel.AlbedoBlue});
            Add(new Channel {Text = "Height", Value = EncodingChannel.Height});
            Add(new Channel {Text = "Occlusion", Value = EncodingChannel.Occlusion});
            Add(new Channel {Text = "Normal-X", Value = EncodingChannel.NormalX});
            Add(new Channel {Text = "Normal-Y", Value = EncodingChannel.NormalY});
            Add(new Channel {Text = "Normal-Z", Value = EncodingChannel.NormalZ});
            Add(new Channel {Text = "Specular", Value = EncodingChannel.Specular});
            Add(new Channel {Text = "Smoothness", Value = EncodingChannel.Smooth});
            Add(new Channel {Text = "Roughness", Value = EncodingChannel.Rough});
            Add(new Channel {Text = "Metalness", Value = EncodingChannel.Metal});
            Add(new Channel {Text = "Porosity", Value = EncodingChannel.Porosity});
            Add(new Channel {Text = "SSS", Value = EncodingChannel.SubSurfaceScattering});
            Add(new Channel {Text = "Emissive", Value = EncodingChannel.Emissive});
            //Add(new Channel {Text = "White", Value = EncodingChannel.White});
        }

        public class Channel
        {
            public string Text {get; set;}
            public string Value {get; set;}
        }
    }
}
