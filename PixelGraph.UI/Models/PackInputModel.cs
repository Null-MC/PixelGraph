using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.UI.Internal;

namespace PixelGraph.UI.Models;

internal class PackInputModel : ModelBase
{
    //public string RootDirectory {get; set;}
    public TextureChannelMapping[] Channels {get;}

    public TextureChannelMapping Opacity {get;}

    public TextureChannelMapping ColorRed {get;}
    public TextureChannelMapping ColorGreen {get;}
    public TextureChannelMapping ColorBlue {get;}

    public TextureChannelMapping Height {get; set;}

    public TextureChannelMapping Occlusion {get; set;}

    public TextureChannelMapping NormalX {get; set;}
    public TextureChannelMapping NormalY {get; set;}
    public TextureChannelMapping NormalZ {get; set;}

    public TextureChannelMapping Specular {get; set;}

    public TextureChannelMapping Smooth {get; set;}
    public TextureChannelMapping Rough {get; set;}

    public TextureChannelMapping Metal {get; set;}
    public TextureChannelMapping F0 {get; set;}

    public TextureChannelMapping Porosity {get; set;}

    public TextureChannelMapping SSS {get; set;}

    public TextureChannelMapping Emissive {get; set;}

    private PackInputEncoding _packInput;

    public PackInputEncoding PackInput {
        get => _packInput;
        set {
            if (_packInput == value) return;
            _packInput = value;
            OnPropertyChanged();

            OnPropertyChanged(nameof(Format));
            UpdateChannels();
            UpdateDefaultValues();
        }
    }

    public string Format {
        get => PackInput?.Format;
        set {
            if (PackInput == null) return;
            if (PackInput.Format == value) return;
            PackInput.Format = value;
            OnPropertyChanged();

            UpdateDefaultValues();
        }
    }


    public PackInputModel()
    {
        Channels = new []{
            Opacity = new TextureChannelMapping("Opacity"),

            ColorRed = new TextureChannelMapping("Color Red"),
            ColorGreen = new TextureChannelMapping("Color Green"),
            ColorBlue = new TextureChannelMapping("Color Blue"),

            Height = new TextureChannelMapping("Height"),

            Occlusion = new TextureChannelMapping("Ambient Occlusion"),

            NormalX = new TextureChannelMapping("Normal-X"),
            NormalY = new TextureChannelMapping("Normal-Y"),
            NormalZ = new TextureChannelMapping("Normal-Z"),

            Specular = new TextureChannelMapping("Specular"),

            Smooth = new TextureChannelMapping("Smooth"),
            Rough = new TextureChannelMapping("Rough"),

            Metal = new TextureChannelMapping("Metal"),
            F0 = new TextureChannelMapping("F0"),

            Porosity = new TextureChannelMapping("Porosity"),

            SSS = new TextureChannelMapping("Sub-Surface-Scattering"),

            Emissive = new TextureChannelMapping("Emissive"),
        };
    }

    public void UpdateDefaultValues()
    {
        var encoding = TextureFormat.GetFactory(_packInput?.Format);
        var encodingDefaults = encoding?.Create();

        Opacity.ApplyDefaultValues(encodingDefaults?.Opacity);

        ColorRed.ApplyDefaultValues(encodingDefaults?.ColorRed);
        ColorGreen.ApplyDefaultValues(encodingDefaults?.ColorGreen);
        ColorBlue.ApplyDefaultValues(encodingDefaults?.ColorBlue);

        Height.ApplyDefaultValues(encodingDefaults?.Height);
        Occlusion.ApplyDefaultValues(encodingDefaults?.Occlusion);

        NormalX.ApplyDefaultValues(encodingDefaults?.NormalX);
        NormalY.ApplyDefaultValues(encodingDefaults?.NormalY);
        NormalZ.ApplyDefaultValues(encodingDefaults?.NormalZ);

        Specular.ApplyDefaultValues(encodingDefaults?.Specular);

        Smooth.ApplyDefaultValues(encodingDefaults?.Smooth);
        Rough.ApplyDefaultValues(encodingDefaults?.Rough);

        Metal.ApplyDefaultValues(encodingDefaults?.Metal);
        F0.ApplyDefaultValues(encodingDefaults?.F0);

        Porosity.ApplyDefaultValues(encodingDefaults?.Porosity);

        SSS.ApplyDefaultValues(encodingDefaults?.SSS);

        Emissive.ApplyDefaultValues(encodingDefaults?.Emissive);
    }

    private void UpdateChannels()
    {
        Opacity.SetChannel(_packInput?.Opacity);

        ColorRed.SetChannel(_packInput?.ColorRed);
        ColorGreen.SetChannel(_packInput?.ColorGreen);
        ColorBlue.SetChannel(_packInput?.ColorBlue);

        Height.SetChannel(_packInput?.Height);
        Occlusion.SetChannel(_packInput?.Occlusion);

        NormalX.SetChannel(_packInput?.NormalX);
        NormalY.SetChannel(_packInput?.NormalY);
        NormalZ.SetChannel(_packInput?.NormalZ);

        Specular.SetChannel(_packInput?.Specular);

        Smooth.SetChannel(_packInput?.Smooth);
        Rough.SetChannel(_packInput?.Rough);

        Metal.SetChannel(_packInput?.Metal);
        F0.SetChannel(_packInput?.F0);

        Porosity.SetChannel(_packInput?.Porosity);

        SSS.SetChannel(_packInput?.SSS);

        Emissive.SetChannel(_packInput?.Emissive);
    }
}

internal class PackInputDesignerModel : PackInputModel
{
    public PackInputDesignerModel()
    {
        PackInput = new PackInputEncoding {
            Format = TextureFormat.Format_Raw,
            Opacity = {
                MinValue = 100,
            },
        };
    }
}