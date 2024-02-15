using PixelGraph.Common.TextureFormats;

namespace PixelGraph.UI.ViewData;

internal class OptionalEncodingChannelValues : List<OptionalEncodingChannelValues.Channel>
{
    public OptionalEncodingChannelValues()
    {
        Add(new Channel {Text = string.Empty});
        Add(new Channel {Text = "Opacity", Value = EncodingChannel.Opacity});
        Add(new Channel {Text = "Color Red", Value = EncodingChannel.ColorRed});
        Add(new Channel {Text = "Color Green", Value = EncodingChannel.ColorGreen});
        Add(new Channel {Text = "Color Blue", Value = EncodingChannel.ColorBlue});
        Add(new Channel {Text = "Height", Value = EncodingChannel.Height});
        Add(new Channel {Text = "Occlusion", Value = EncodingChannel.Occlusion});
        Add(new Channel {Text = "Normal-X", Value = EncodingChannel.NormalX});
        Add(new Channel {Text = "Normal-Y", Value = EncodingChannel.NormalY});
        Add(new Channel {Text = "Normal-Z", Value = EncodingChannel.NormalZ});
        Add(new Channel {Text = "Specular", Value = EncodingChannel.Specular});
        Add(new Channel {Text = "Smoothness", Value = EncodingChannel.Smooth});
        Add(new Channel {Text = "Roughness", Value = EncodingChannel.Rough});
        Add(new Channel {Text = "Metalness", Value = EncodingChannel.Metal});
        Add(new Channel {Text = "HCM", Value = EncodingChannel.HCM});
        Add(new Channel {Text = "F0", Value = EncodingChannel.F0});
        Add(new Channel {Text = "Porosity", Value = EncodingChannel.Porosity});
        Add(new Channel {Text = "SSS", Value = EncodingChannel.SubSurfaceScattering});
        Add(new Channel {Text = "Emissive", Value = EncodingChannel.Emissive});
        //Add(new Channel {Text = "White", Value = EncodingChannel.White});
    }

    public class Channel
    {
        public string? Text {get; set;}
        public string? Value {get; set;}
    }
}