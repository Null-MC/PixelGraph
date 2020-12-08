using PixelGraph.Common.Encoding;
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
            if (encoding.Alpha.HasMapping) Alpha = encoding.Alpha;

            if (encoding.DiffuseRed.HasMapping) DiffuseRed = encoding.DiffuseRed;
            if (encoding.DiffuseGreen.HasMapping) DiffuseGreen = encoding.DiffuseGreen;
            if (encoding.DiffuseBlue.HasMapping) DiffuseBlue = encoding.DiffuseBlue;

            if (encoding.AlbedoRed.HasMapping) AlbedoRed = encoding.AlbedoRed;
            if (encoding.AlbedoGreen.HasMapping) AlbedoGreen = encoding.AlbedoGreen;
            if (encoding.AlbedoBlue.HasMapping) AlbedoBlue = encoding.AlbedoBlue;

            if (encoding.Height.HasMapping) Height = encoding.Height;
            if (encoding.Occlusion.HasMapping) Occlusion = encoding.Occlusion;

            if (encoding.NormalX.HasMapping) NormalX = encoding.NormalX;
            if (encoding.NormalY.HasMapping) NormalY = encoding.NormalY;
            if (encoding.NormalZ.HasMapping) NormalZ = encoding.NormalZ;

            if (encoding.Specular.HasMapping) Specular = encoding.Specular;

            if (encoding.Smooth.HasMapping) Smooth = encoding.Smooth;
            if (encoding.Rough.HasMapping) Rough = encoding.Rough;

            if (encoding.Metal.HasMapping) Metal = encoding.Metal;

            if (encoding.Porosity.HasMapping) Porosity = encoding.Porosity;

            if (encoding.SSS.HasMapping) SSS = encoding.SSS;

            if (encoding.Emissive.HasMapping) Emissive = encoding.Emissive;
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

            yield return Porosity;

            yield return SSS;

            yield return Emissive;
        }
    }

    public class ResourcePackAlphaChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackAlphaChannelProperties() : base(EncodingChannel.Alpha) {}
    }

    public class ResourcePackDiffuseRedChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackDiffuseRedChannelProperties() : base(EncodingChannel.DiffuseRed) {}
    }

    public class ResourcePackDiffuseGreenChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackDiffuseGreenChannelProperties() : base(EncodingChannel.DiffuseGreen) {}
    }

    public class ResourcePackDiffuseBlueChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackDiffuseBlueChannelProperties() : base(EncodingChannel.DiffuseBlue) {}
    }

    public class ResourcePackAlbedoRedChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackAlbedoRedChannelProperties() : base(EncodingChannel.AlbedoRed) {}
    }

    public class ResourcePackAlbedoGreenChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackAlbedoGreenChannelProperties() : base(EncodingChannel.AlbedoGreen) {}
    }

    public class ResourcePackAlbedoBlueChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackAlbedoBlueChannelProperties() : base(EncodingChannel.AlbedoBlue) {}
    }

    public class ResourcePackHeightChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackHeightChannelProperties() : base(EncodingChannel.Height) {}
    }

    public class ResourcePackOcclusionChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackOcclusionChannelProperties() : base(EncodingChannel.Occlusion) {}
    }

    public class ResourcePackNormalXChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackNormalXChannelProperties() : base(EncodingChannel.NormalX) {}
    }

    public class ResourcePackNormalYChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackNormalYChannelProperties() : base(EncodingChannel.NormalY) {}
    }

    public class ResourcePackNormalZChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackNormalZChannelProperties() : base(EncodingChannel.NormalZ) {}
    }

    public class ResourcePackSpecularChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackSpecularChannelProperties() : base(EncodingChannel.Specular) {}
    }

    public class ResourcePackSmoothChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackSmoothChannelProperties() : base(EncodingChannel.Smooth) {}
    }

    public class ResourcePackRoughChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackRoughChannelProperties() : base(EncodingChannel.Rough) {}
    }

    public class ResourcePackMetalChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackMetalChannelProperties() : base(EncodingChannel.Metal) {}
    }

    public class ResourcePackPorosityChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackPorosityChannelProperties() : base(EncodingChannel.Porosity) {}
    }

    public class ResourcePackSssChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackSssChannelProperties() : base(EncodingChannel.SubSurfaceScattering) {}
    }

    public class ResourcePackEmissiveChannelProperties : ResourcePackChannelProperties
    {
        public ResourcePackEmissiveChannelProperties() : base(EncodingChannel.Emissive) {}
    }
}
