using PixelGraph.Common.Encoding;
using PixelGraph.Common.Material;
using PixelGraph.Common.Textures;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace PixelGraph.Common.ResourcePack
{
    public class ResourcePackEncoding
    {
        public ResourcePackAlphaChannelProperties Alpha {get; set;}

        public ResourcePackDiffuseRedChannelProperties DiffuseRed {get; set;}
        public ResourcePackDiffuseGreenChannelProperties DiffuseGreen {get; set;}
        public ResourcePackDiffuseBlueChannelProperties DiffuseBlue {get; set;}

        public ResourcePackAlbedoRedChannelProperties AlbedoRed {get; set;}
        public ResourcePackAlbedoGreenChannelProperties AlbedoGreen {get; set;}
        public ResourcePackAlbedoBlueChannelProperties AlbedoBlue {get; set;}

        public ResourcePackHeightChannelProperties Height {get; set;}
        public ResourcePackOcclusionChannelProperties Occlusion {get; set;}

        public ResourcePackNormalXChannelProperties NormalX {get; set;}
        public ResourcePackNormalYChannelProperties NormalY {get; set;}
        public ResourcePackNormalZChannelProperties NormalZ {get; set;}

        public ResourcePackSpecularChannelProperties Specular {get; set;}

        public ResourcePackSmoothChannelProperties Smooth {get; set;}
        public ResourcePackRoughChannelProperties Rough {get; set;}

        public ResourcePackMetalChannelProperties Metal {get; set;}
        public ResourcePackF0ChannelProperties F0 {get; set;}

        public ResourcePackPorosityChannelProperties Porosity {get; set;}

        [YamlMember(Alias = "sss")]
        public ResourcePackSssChannelProperties SSS {get; set;}

        public ResourcePackEmissiveChannelProperties Emissive {get; set;}


        public ResourcePackEncoding()
        {
            Alpha = new ResourcePackAlphaChannelProperties();

            DiffuseRed = new ResourcePackDiffuseRedChannelProperties();
            DiffuseGreen = new ResourcePackDiffuseGreenChannelProperties();
            DiffuseBlue = new ResourcePackDiffuseBlueChannelProperties();

            AlbedoRed = new ResourcePackAlbedoRedChannelProperties();
            AlbedoGreen = new ResourcePackAlbedoGreenChannelProperties();
            AlbedoBlue = new ResourcePackAlbedoBlueChannelProperties();

            Height = new ResourcePackHeightChannelProperties();

            Occlusion = new ResourcePackOcclusionChannelProperties();

            NormalX = new ResourcePackNormalXChannelProperties();
            NormalY = new ResourcePackNormalYChannelProperties();
            NormalZ = new ResourcePackNormalZChannelProperties();

            Specular = new ResourcePackSpecularChannelProperties();

            Smooth = new ResourcePackSmoothChannelProperties();
            Rough = new ResourcePackRoughChannelProperties();

            Metal = new ResourcePackMetalChannelProperties();
            F0 = new ResourcePackF0ChannelProperties();

            Porosity = new ResourcePackPorosityChannelProperties();

            SSS = new ResourcePackSssChannelProperties();

            Emissive = new ResourcePackEmissiveChannelProperties();
        }

        public IEnumerable<ResourcePackChannelProperties> GetMapped()
        {
            return GetAll().Where(e => e.HasMapping);
        }

        public void Merge(ResourcePackEncoding encoding)
        {
            if (encoding.Alpha != null) Alpha.Merge(encoding.Alpha);

            if (encoding.DiffuseRed != null) DiffuseRed.Merge(encoding.DiffuseRed);
            if (encoding.DiffuseGreen != null) DiffuseGreen.Merge(encoding.DiffuseGreen);
            if (encoding.DiffuseBlue != null) DiffuseBlue.Merge(encoding.DiffuseBlue);

            if (encoding.AlbedoRed != null) AlbedoRed.Merge(encoding.AlbedoRed);
            if (encoding.AlbedoGreen != null) AlbedoGreen.Merge(encoding.AlbedoGreen);
            if (encoding.AlbedoBlue != null) AlbedoBlue.Merge(encoding.AlbedoBlue);

            if (encoding.Height != null) Height.Merge(encoding.Height);
            if (encoding.Occlusion != null) Occlusion.Merge(encoding.Occlusion);

            if (encoding.NormalX != null) NormalX.Merge(encoding.NormalX);
            if (encoding.NormalY != null) NormalY.Merge(encoding.NormalY);
            if (encoding.NormalZ != null) NormalZ.Merge(encoding.NormalZ);

            if (encoding.Specular != null) Specular.Merge(encoding.Specular);

            if (encoding.Smooth != null) Smooth.Merge(encoding.Smooth);
            if (encoding.Rough != null) Rough.Merge(encoding.Rough);

            if (encoding.Metal != null) Metal.Merge(encoding.Metal);
            if (encoding.F0 != null) F0.Merge(encoding.F0);

            if (encoding.Porosity != null) Porosity.Merge(encoding.Porosity);

            if (encoding.SSS != null) SSS.Merge(encoding.SSS);

            if (encoding.Emissive != null) Emissive.Merge(encoding.Emissive);
        }

        public void Merge(MaterialProperties material)
        {
            if (material.Alpha?.Input != null) Alpha.Merge(material.Alpha.Input);

            if (material.Diffuse?.InputRed != null) DiffuseRed.Merge(material.Diffuse.InputRed);
            if (material.Diffuse?.InputGreen != null) DiffuseGreen.Merge(material.Diffuse.InputGreen);
            if (material.Diffuse?.InputBlue != null) DiffuseBlue.Merge(material.Diffuse.InputBlue);

            if (material.Albedo?.InputRed != null) AlbedoRed.Merge(material.Albedo.InputRed);
            if (material.Albedo?.InputGreen != null) AlbedoGreen.Merge(material.Albedo.InputGreen);
            if (material.Albedo?.InputBlue != null) AlbedoBlue.Merge(material.Albedo.InputBlue);

            if (material.Height?.Input != null) Height.Merge(material.Height.Input);
            if (material.Occlusion?.Input != null) Occlusion.Merge(material.Occlusion.Input);

            if (material.Normal?.InputX != null) NormalX.Merge(material.Normal.InputX);
            if (material.Normal?.InputY != null) NormalY.Merge(material.Normal.InputY);
            if (material.Normal?.InputZ != null) NormalZ.Merge(material.Normal.InputZ);

            if (material.Specular?.Input != null) Specular.Merge(material.Specular.Input);

            if (material.Smooth?.Input != null) Smooth.Merge(material.Smooth.Input);
            if (material.Rough?.Input != null) Rough.Merge(material.Rough.Input);

            if (material.Metal?.Input != null) Metal.Merge(material.Metal.Input);
            if (material.F0?.Input != null) F0.Merge(material.F0.Input);

            if (material.Porosity?.Input != null) Porosity.Merge(material.Porosity.Input);

            if (material.SSS?.Input != null) SSS.Merge(material.SSS.Input);

            if (material.Emissive?.Input != null) Emissive.Merge(material.Emissive.Input);
        }

        private IEnumerable<ResourcePackChannelProperties> GetAll()
        {
            yield return Alpha;

            yield return AlbedoRed;
            yield return AlbedoGreen;
            yield return AlbedoBlue;

            yield return DiffuseRed;
            yield return DiffuseGreen;
            yield return DiffuseBlue;

            yield return Height;
            yield return Occlusion;

            yield return NormalX;
            yield return NormalY;
            yield return NormalZ;

            yield return Specular;

            yield return Smooth;
            yield return Rough;

            yield return Metal;
            yield return F0;

            yield return Porosity;

            yield return SSS;

            yield return Emissive;
        }

        public virtual object Clone()
        {
            var clone = (ResourcePackEncoding)MemberwiseClone();

            clone.Alpha = (ResourcePackAlphaChannelProperties)Alpha.Clone();

            clone.DiffuseRed = (ResourcePackDiffuseRedChannelProperties)DiffuseRed.Clone();
            clone.DiffuseGreen = (ResourcePackDiffuseGreenChannelProperties)DiffuseGreen.Clone();
            clone.DiffuseBlue = (ResourcePackDiffuseBlueChannelProperties)DiffuseBlue.Clone();

            clone.AlbedoRed = (ResourcePackAlbedoRedChannelProperties)AlbedoRed.Clone();
            clone.AlbedoGreen = (ResourcePackAlbedoGreenChannelProperties)AlbedoGreen.Clone();
            clone.AlbedoBlue = (ResourcePackAlbedoBlueChannelProperties)AlbedoBlue.Clone();

            clone.Height = (ResourcePackHeightChannelProperties)Height.Clone();
            clone.Occlusion = (ResourcePackOcclusionChannelProperties)Occlusion.Clone();

            clone.NormalX = (ResourcePackNormalXChannelProperties)NormalX.Clone();
            clone.NormalY = (ResourcePackNormalYChannelProperties)NormalY.Clone();
            clone.NormalZ = (ResourcePackNormalZChannelProperties)NormalZ.Clone();

            clone.Specular = (ResourcePackSpecularChannelProperties)Specular.Clone();

            clone.Smooth = (ResourcePackSmoothChannelProperties)Smooth.Clone();
            clone.Rough = (ResourcePackRoughChannelProperties)Rough.Clone();

            clone.Metal = (ResourcePackMetalChannelProperties)Metal.Clone();
            clone.F0 = (ResourcePackF0ChannelProperties)F0.Clone();

            clone.Porosity = (ResourcePackPorosityChannelProperties)Porosity.Clone();

            clone.SSS = (ResourcePackSssChannelProperties)SSS.Clone();

            clone.Emissive = (ResourcePackEmissiveChannelProperties)Emissive.Clone();

            return clone;
        }
    }

    public class ResourcePackAlphaChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackAlphaChannelProperties() : base(EncodingChannel.Alpha) {}

        public ResourcePackAlphaChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.Alpha, texture, color) {}

        public override object Clone()
        {
            return MemberwiseClone();
        }
    }

    public class ResourcePackDiffuseRedChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackDiffuseRedChannelProperties() : base(EncodingChannel.DiffuseRed) {}

        public ResourcePackDiffuseRedChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.DiffuseRed, texture, color) {}
    }

    public class ResourcePackDiffuseGreenChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackDiffuseGreenChannelProperties() : base(EncodingChannel.DiffuseGreen) {}

        public ResourcePackDiffuseGreenChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.DiffuseGreen, texture, color) {}
    }

    public class ResourcePackDiffuseBlueChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackDiffuseBlueChannelProperties() : base(EncodingChannel.DiffuseBlue) {}

        public ResourcePackDiffuseBlueChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.DiffuseBlue, texture, color) {}
    }

    public class ResourcePackAlbedoRedChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackAlbedoRedChannelProperties() : base(EncodingChannel.AlbedoRed) {}

        public ResourcePackAlbedoRedChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.AlbedoRed, texture, color) {}
    }

    public class ResourcePackAlbedoGreenChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackAlbedoGreenChannelProperties() : base(EncodingChannel.AlbedoGreen) {}

        public ResourcePackAlbedoGreenChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.AlbedoGreen, texture, color) {}
    }

    public class ResourcePackAlbedoBlueChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackAlbedoBlueChannelProperties() : base(EncodingChannel.AlbedoBlue) {}

        public ResourcePackAlbedoBlueChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.AlbedoBlue, texture, color) {}
    }

    public class ResourcePackHeightChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackHeightChannelProperties() : base(EncodingChannel.Height) {}

        public ResourcePackHeightChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.Height, texture, color) {}
    }

    public class ResourcePackBumpChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackBumpChannelProperties() : base(EncodingChannel.Bump) {}

        public ResourcePackBumpChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.Bump, texture, color) {}
    }

    public class ResourcePackOcclusionChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackOcclusionChannelProperties() : base(EncodingChannel.Occlusion) {}

        public ResourcePackOcclusionChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.Occlusion, texture, color) {}
    }

    public class ResourcePackNormalXChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackNormalXChannelProperties() : base(EncodingChannel.NormalX) {}

        public ResourcePackNormalXChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.NormalX, texture, color) {}
    }

    public class ResourcePackNormalYChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackNormalYChannelProperties() : base(EncodingChannel.NormalY) {}

        public ResourcePackNormalYChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.NormalY, texture, color) {}
    }

    public class ResourcePackNormalZChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackNormalZChannelProperties() : base(EncodingChannel.NormalZ) {}

        public ResourcePackNormalZChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.NormalZ, texture, color) {}
    }

    public class ResourcePackSpecularChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackSpecularChannelProperties() : base(EncodingChannel.Specular) {}

        public ResourcePackSpecularChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.Specular, texture, color) {}
    }

    public class ResourcePackSmoothChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackSmoothChannelProperties() : base(EncodingChannel.Smooth) {}

        public ResourcePackSmoothChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.Smooth, texture, color) {}
    }

    public class ResourcePackRoughChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackRoughChannelProperties() : base(EncodingChannel.Rough) {}

        public ResourcePackRoughChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.Rough, texture, color) {}
    }

    public class ResourcePackMetalChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackMetalChannelProperties() : base(EncodingChannel.Metal) {}

        public ResourcePackMetalChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.Metal, texture, color) {}
    }

    public class ResourcePackF0ChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackF0ChannelProperties() : base(EncodingChannel.F0) {}

        public ResourcePackF0ChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.F0, texture, color) {}
    }

    public class ResourcePackPorosityChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackPorosityChannelProperties() : base(EncodingChannel.Porosity) {}

        public ResourcePackPorosityChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.Porosity, texture, color) {}
    }

    public class ResourcePackSssChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackSssChannelProperties() : base(EncodingChannel.SubSurfaceScattering) {}

        public ResourcePackSssChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.SubSurfaceScattering, texture, color) {}
    }

    public class ResourcePackEmissiveChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackEmissiveChannelProperties() : base(EncodingChannel.Emissive) {}

        public ResourcePackEmissiveChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.Emissive, texture, color) {}
    }
}
