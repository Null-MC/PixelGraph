using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;

namespace PixelGraph.UI.ViewModels
{
    internal class PackInputVM : ViewModelBase
    {
        public string RootDirectory {get; set;}
        public TextureChannelMapping[] Channels {get;}

        public TextureChannelMapping Alpha {get;}

        public TextureChannelMapping DiffuseRed {get;}
        public TextureChannelMapping DiffuseGreen {get;}
        public TextureChannelMapping DiffuseBlue {get;}

        public TextureChannelMapping AlbedoRed {get;}
        public TextureChannelMapping AlbedoGreen {get;}
        public TextureChannelMapping AlbedoBlue {get;}

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

        private ResourcePackInputProperties _packInput;

        public ResourcePackInputProperties PackInput {
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


        public PackInputVM()
        {
            Channels = new []{
                Alpha = new TextureChannelMapping("Alpha"),

                DiffuseRed = new TextureChannelMapping("Diffuse Red"),
                DiffuseGreen = new TextureChannelMapping("Diffuse Green"),
                DiffuseBlue = new TextureChannelMapping("Diffuse Blue"),

                AlbedoRed = new TextureChannelMapping("Albedo Red"),
                AlbedoGreen = new TextureChannelMapping("Albedo Green"),
                AlbedoBlue = new TextureChannelMapping("Albedo Blue"),

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
            var encoding = TextureEncoding.GetFactory(_packInput?.Format);
            var encodingDefaults = encoding?.Create();

            Alpha.ApplyDefaultValues(encodingDefaults?.Alpha);

            DiffuseRed.ApplyDefaultValues(encodingDefaults?.DiffuseRed);
            DiffuseGreen.ApplyDefaultValues(encodingDefaults?.DiffuseGreen);
            DiffuseBlue.ApplyDefaultValues(encodingDefaults?.DiffuseBlue);

            AlbedoRed.ApplyDefaultValues(encodingDefaults?.AlbedoRed);
            AlbedoGreen.ApplyDefaultValues(encodingDefaults?.AlbedoGreen);
            AlbedoBlue.ApplyDefaultValues(encodingDefaults?.AlbedoBlue);

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
            Alpha.SetChannel(_packInput?.Alpha);

            DiffuseRed.SetChannel(_packInput?.DiffuseRed);
            DiffuseGreen.SetChannel(_packInput?.DiffuseGreen);
            DiffuseBlue.SetChannel(_packInput?.DiffuseBlue);

            AlbedoRed.SetChannel(_packInput?.AlbedoRed);
            AlbedoGreen.SetChannel(_packInput?.AlbedoGreen);
            AlbedoBlue.SetChannel(_packInput?.AlbedoBlue);

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

    internal class PackInput2DesignVM : PackInputVM
    {
        public PackInput2DesignVM()
        {
            PackInput = new ResourcePackInputProperties {
                Format = TextureEncoding.Format_Raw,
                Alpha = {
                    MinValue = 100,
                },
            };
        }
    }
}
