using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;

namespace PixelGraph.UI.ViewModels
{
    internal class PackInput2VM : ViewModelBase
    {
        public string RootDirectory {get; set;}
        public ResourcePackInputProperties PackInput {get; set;}
        public EncodingChannelMapping[] Channels {get;}

        public EncodingChannelMapping Alpha {get; set;}
        public EncodingChannelMapping DiffuseRed {get; set;}
        public EncodingChannelMapping DiffuseGreen {get; set;}
        public EncodingChannelMapping DiffuseBlue {get; set;}
        public EncodingChannelMapping AlbedoRed {get; set;}
        public EncodingChannelMapping AlbedoGreen {get; set;}
        public EncodingChannelMapping AlbedoBlue {get; set;}

        public EncodingChannelMapping Height {get; set;}

        public EncodingChannelMapping Occlusion {get; set;}

        public EncodingChannelMapping NormalX {get; set;}
        public EncodingChannelMapping NormalY {get; set;}
        public EncodingChannelMapping NormalZ {get; set;}

        public EncodingChannelMapping Specular {get; set;}

        public EncodingChannelMapping Smooth {get; set;}
        public EncodingChannelMapping Rough {get; set;}

        public EncodingChannelMapping Metal {get; set;}

        public EncodingChannelMapping Porosity {get; set;}

        public EncodingChannelMapping SSS {get; set;}

        public EncodingChannelMapping Emissive {get; set;}

        public string Format {
            get => PackInput.Format;
            set {
                PackInput.Format = value;
                OnPropertyChanged();

                OnFormatChanged();
            }
        }


        public PackInput2VM()
        {
            Channels = new []{
                Alpha = new EncodingChannelMapping("Alpha"),

                DiffuseRed = new EncodingChannelMapping("Diffuse Red"),
                DiffuseGreen = new EncodingChannelMapping("Diffuse Green"),
                DiffuseBlue = new EncodingChannelMapping("Diffuse Blue"),

                AlbedoRed = new EncodingChannelMapping("Albedo Red"),
                AlbedoGreen = new EncodingChannelMapping("Albedo Green"),
                AlbedoBlue = new EncodingChannelMapping("Albedo Blue"),

                Height = new EncodingChannelMapping("Height"),

                Occlusion = new EncodingChannelMapping("Ambient Occlusion"),

                NormalX = new EncodingChannelMapping("Normal-X"),
                NormalY = new EncodingChannelMapping("Normal-Y"),
                NormalZ = new EncodingChannelMapping("Normal-Z"),

                Specular = new EncodingChannelMapping("Specular"),

                Smooth = new EncodingChannelMapping("Smooth"),
                Rough = new EncodingChannelMapping("Rough"),

                Metal = new EncodingChannelMapping("Metal"),

                Porosity = new EncodingChannelMapping("Porosity"),

                SSS = new EncodingChannelMapping("Sub-Surface-Scattering"),

                Emissive = new EncodingChannelMapping("Emissive"),
            };
        }

        private void OnFormatChanged()
        {
            // TODO: save input

            UpdateDefaultValues();
        }

        private void UpdateDefaultValues()
        {
            var encodingDefaults = new ResourcePackEncoding();
            var encoding = TextureEncoding.GetFormat(Format);
            encoding.Apply(encodingDefaults);

            Alpha.ApplyDefaultValues(encodingDefaults.Alpha);

            DiffuseRed.ApplyDefaultValues(encodingDefaults.DiffuseRed);
            DiffuseGreen.ApplyDefaultValues(encodingDefaults.DiffuseGreen);
            DiffuseBlue.ApplyDefaultValues(encodingDefaults.DiffuseBlue);

            AlbedoRed.ApplyDefaultValues(encodingDefaults.AlbedoRed);
            AlbedoGreen.ApplyDefaultValues(encodingDefaults.AlbedoGreen);
            AlbedoBlue.ApplyDefaultValues(encodingDefaults.AlbedoBlue);

            Height.ApplyDefaultValues(encodingDefaults.Height);
            Occlusion.ApplyDefaultValues(encodingDefaults.Occlusion);

            NormalX.ApplyDefaultValues(encodingDefaults.NormalX);
            NormalY.ApplyDefaultValues(encodingDefaults.NormalY);
            NormalZ.ApplyDefaultValues(encodingDefaults.NormalZ);

            Specular.ApplyDefaultValues(encodingDefaults.Specular);

            Smooth.ApplyDefaultValues(encodingDefaults.Smooth);
            Rough.ApplyDefaultValues(encodingDefaults.Rough);

            Metal.ApplyDefaultValues(encodingDefaults.Metal);

            Porosity.ApplyDefaultValues(encodingDefaults.Porosity);

            SSS.ApplyDefaultValues(encodingDefaults.SSS);

            Emissive.ApplyDefaultValues(encodingDefaults.Emissive);
        }
    }

    internal class PackInput2DesignVM : PackInput2VM
    {
        public PackInput2DesignVM()
        {
            Alpha.Texture = TextureTags.Albedo;
            Alpha.Color = ColorChannel.Alpha;
            Alpha.MinDefault = 0;
            Alpha.MaxDefault = 255;
            Alpha.MinValue = 100;

            AlbedoRed.Texture = TextureTags.Albedo;
            AlbedoRed.Color = ColorChannel.Red;
        }
    }

    internal class EncodingChannelMapping
    {
        public string Label {get;}
        public string Texture {get; set;}
        public string TextureDefault {get; set;}
        public ColorChannel? Color {get; set;}
        public ColorChannel? ColorDefault {get; set;}
        public byte? MinDefault {get; set;}
        public byte? MaxDefault {get; set;}
        public byte? MinValue {get; set;}
        public byte? MaxValue {get; set;}


        public EncodingChannelMapping(string label)
        {
            Label = label;
        }

        public void ApplyDefaultValues(ResourcePackChannelProperties encodingDefaults)
        {
            TextureDefault = encodingDefaults.Texture;
            ColorDefault = encodingDefaults.Color;
            MinDefault = encodingDefaults.MinValue;
            MaxDefault = encodingDefaults.MaxValue;
        }
    }
}
