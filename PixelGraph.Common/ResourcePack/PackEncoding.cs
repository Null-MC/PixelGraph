﻿using Nito.Disposables.Internals;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Material;
using PixelGraph.Common.TextureFormats;
using PixelGraph.Common.Textures;
using YamlDotNet.Serialization;

namespace PixelGraph.Common.ResourcePack;

public class PackEncoding : IHaveData
{
    public ResourcePackOpacityChannelProperties Opacity {get; set;}

    public ResourcePackColorRedChannelProperties ColorRed {get; set;}
    public ResourcePackColorGreenChannelProperties ColorGreen {get; set;}
    public ResourcePackColorBlueChannelProperties ColorBlue {get; set;}

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

    [YamlMember(Alias = "hcm")]
    public ResourcePackHcmChannelProperties HCM {get; set;}

    public ResourcePackPorosityChannelProperties Porosity {get; set;}

    [YamlMember(Alias = "sss")]
    public ResourcePackSssChannelProperties SSS {get; set;}

    public ResourcePackEmissiveChannelProperties Emissive {get; set;}


    //public static PackEncoding New()
    //{
    //    return new PackEncoding {
    //        Opacity = new ResourcePackOpacityChannelProperties(),

    //        ColorRed = new ResourcePackColorRedChannelProperties(),
    //        ColorGreen = new ResourcePackColorGreenChannelProperties(),
    //        ColorBlue = new ResourcePackColorBlueChannelProperties(),

    //        Height = new ResourcePackHeightChannelProperties(),

    //        Occlusion = new ResourcePackOcclusionChannelProperties(),

    //        NormalX = new ResourcePackNormalXChannelProperties(),
    //        NormalY = new ResourcePackNormalYChannelProperties(),
    //        NormalZ = new ResourcePackNormalZChannelProperties(),

    //        Specular = new ResourcePackSpecularChannelProperties(),

    //        Smooth = new ResourcePackSmoothChannelProperties(),
    //        Rough = new ResourcePackRoughChannelProperties(),

    //        Metal = new ResourcePackMetalChannelProperties(),
    //        HCM = new ResourcePackHcmChannelProperties(),
    //        F0 = new ResourcePackF0ChannelProperties(),

    //        Porosity = new ResourcePackPorosityChannelProperties(),

    //        SSS = new ResourcePackSssChannelProperties(),

    //        Emissive = new ResourcePackEmissiveChannelProperties(),
    //    };
    //}

    public PackEncoding()
    {
        Opacity = new ResourcePackOpacityChannelProperties();

        ColorRed = new ResourcePackColorRedChannelProperties();
        ColorGreen = new ResourcePackColorGreenChannelProperties();
        ColorBlue = new ResourcePackColorBlueChannelProperties();

        Height = new ResourcePackHeightChannelProperties();

        Occlusion = new ResourcePackOcclusionChannelProperties();

        NormalX = new ResourcePackNormalXChannelProperties();
        NormalY = new ResourcePackNormalYChannelProperties();
        NormalZ = new ResourcePackNormalZChannelProperties();

        Specular = new ResourcePackSpecularChannelProperties();

        Smooth = new ResourcePackSmoothChannelProperties();
        Rough = new ResourcePackRoughChannelProperties();

        Metal = new ResourcePackMetalChannelProperties();
        HCM = new ResourcePackHcmChannelProperties();
        F0 = new ResourcePackF0ChannelProperties();

        Porosity = new ResourcePackPorosityChannelProperties();

        SSS = new ResourcePackSssChannelProperties();

        Emissive = new ResourcePackEmissiveChannelProperties();
    }

    //public ResourcePackChannelProperties Get(string encodingChannel)
    //{
    //    return GetAll().FirstOrDefault(e => EncodingChannel.Is(e.ID, encodingChannel));
    //}

    public IEnumerable<PackEncodingChannel> GetMapped()
    {
        return GetAll().WhereNotNull().Where(e => e.HasMapping);
    }

    public void Merge(PackEncoding encoding)
    {
        Opacity.Merge(encoding.Opacity);
        ColorRed.Merge(encoding.ColorRed);
        ColorGreen.Merge(encoding.ColorGreen);
        ColorBlue.Merge(encoding.ColorBlue);
        Height.Merge(encoding.Height);
        Occlusion.Merge(encoding.Occlusion);
        NormalX.Merge(encoding.NormalX);
        NormalY.Merge(encoding.NormalY);
        NormalZ.Merge(encoding.NormalZ);
        Specular.Merge(encoding.Specular);
        Smooth.Merge(encoding.Smooth);
        Rough.Merge(encoding.Rough);
        Metal.Merge(encoding.Metal);
        HCM.Merge(encoding.HCM);
        F0.Merge(encoding.F0);
        Porosity.Merge(encoding.Porosity);
        SSS.Merge(encoding.SSS);
        Emissive.Merge(encoding.Emissive);
    }

    public void Merge(MaterialProperties material)
    {
        if (material.Opacity?.Input != null)
            Opacity.Merge(material.Opacity.Input);

        if (material.Color?.InputRed != null)
            ColorRed.Merge(material.Color.InputRed);

        if (material.Color?.InputGreen != null)
            ColorGreen.Merge(material.Color.InputGreen);

        if (material.Color?.InputBlue != null)
            ColorBlue.Merge(material.Color.InputBlue);

        if (material.Height?.Input != null)
            Height.Merge(material.Height.Input);

        if (material.Occlusion?.Input != null)
            Occlusion.Merge(material.Occlusion.Input);

        if (material.Normal?.InputX != null)
            NormalX.Merge(material.Normal.InputX);

        if (material.Normal?.InputY != null)
            NormalY.Merge(material.Normal.InputY);

        if (material.Normal?.InputZ != null)
            NormalZ.Merge(material.Normal.InputZ);

        if (material.Specular?.Input != null)
            Specular.Merge(material.Specular.Input);

        if (material.Smooth?.Input != null)
            Smooth.Merge(material.Smooth.Input);

        if (material.Rough?.Input != null)
            Rough.Merge(material.Rough.Input);

        if (material.Metal?.Input != null)
            Metal.Merge(material.Metal.Input);

        if (material.HCM?.Input != null)
            HCM.Merge(material.HCM.Input);

        if (material.F0?.Input != null)
            F0.Merge(material.F0.Input);

        if (material.Porosity?.Input != null)
            Porosity.Merge(material.Porosity.Input);

        if (material.SSS?.Input != null)
            SSS.Merge(material.SSS.Input);

        if (material.Emissive?.Input != null)
            Emissive.Merge(material.Emissive.Input);
    }

    private IEnumerable<PackEncodingChannel?> GetAll()
    {
        yield return Opacity;

        yield return ColorRed;
        yield return ColorGreen;
        yield return ColorBlue;

        yield return Height;
        yield return Occlusion;

        yield return NormalX;
        yield return NormalY;
        yield return NormalZ;

        yield return Specular;

        yield return Smooth;
        yield return Rough;

        yield return Metal;
        yield return HCM;
        yield return F0;

        yield return Porosity;

        yield return SSS;

        yield return Emissive;
    }

    public virtual object Clone()
    {
        var clone = (PackEncoding)MemberwiseClone();

        clone.Opacity = (ResourcePackOpacityChannelProperties)Opacity.Clone();

        clone.ColorRed = (ResourcePackColorRedChannelProperties)ColorRed.Clone();
        clone.ColorGreen = (ResourcePackColorGreenChannelProperties)ColorGreen.Clone();
        clone.ColorBlue = (ResourcePackColorBlueChannelProperties)ColorBlue.Clone();

        clone.Height = (ResourcePackHeightChannelProperties)Height.Clone();
        clone.Occlusion = (ResourcePackOcclusionChannelProperties)Occlusion.Clone();

        clone.NormalX = (ResourcePackNormalXChannelProperties)NormalX.Clone();
        clone.NormalY = (ResourcePackNormalYChannelProperties)NormalY.Clone();
        clone.NormalZ = (ResourcePackNormalZChannelProperties)NormalZ.Clone();

        clone.Specular = (ResourcePackSpecularChannelProperties)Specular.Clone();

        clone.Smooth = (ResourcePackSmoothChannelProperties)Smooth.Clone();
        clone.Rough = (ResourcePackRoughChannelProperties)Rough.Clone();

        clone.Metal = (ResourcePackMetalChannelProperties)Metal.Clone();
        clone.HCM = (ResourcePackHcmChannelProperties)HCM.Clone();
        clone.F0 = (ResourcePackF0ChannelProperties)F0.Clone();

        clone.Porosity = (ResourcePackPorosityChannelProperties)Porosity.Clone();

        clone.SSS = (ResourcePackSssChannelProperties)SSS.Clone();

        clone.Emissive = (ResourcePackEmissiveChannelProperties)Emissive.Clone();

        return clone;
    }

    public virtual bool HasAnyData()
    {
        return GetAll().WhereNotNull().Any(channel => channel.HasAnyData());
    }

    protected static readonly Dictionary<string, Func<PackEncoding, PackEncodingChannel>> channelMap =
        new(StringComparer.InvariantCultureIgnoreCase) {
            [EncodingChannel.Opacity] = e => e.Opacity,
            [EncodingChannel.ColorRed] = e => e.ColorRed,
            [EncodingChannel.ColorGreen] = e => e.ColorGreen,
            [EncodingChannel.ColorBlue] = e => e.ColorBlue,
            [EncodingChannel.Height] = e => e.Height,
            [EncodingChannel.Occlusion] = e => e.Occlusion,
            [EncodingChannel.NormalX] = e => e.NormalX,
            [EncodingChannel.NormalY] = e => e.NormalY,
            [EncodingChannel.NormalZ] = e => e.NormalZ,
            [EncodingChannel.Specular] = e => e.Specular,
            [EncodingChannel.Smooth] = e => e.Smooth,
            [EncodingChannel.Rough] = e => e.Rough,
            [EncodingChannel.Metal] = e => e.Metal,
            [EncodingChannel.HCM] = e => e.HCM,
            [EncodingChannel.F0] = e => e.F0,
            [EncodingChannel.Porosity] = e => e.Porosity,
            [EncodingChannel.SubSurfaceScattering] = e => e.SSS,
            [EncodingChannel.Emissive] = e => e.Emissive,
        };

    #region Deprecated

    [Obsolete("Replace usages of Alpha with Opacity.")]
    public ResourcePackOpacityChannelProperties? Alpha {
        get => null;
        set => Opacity = value ?? throw new ArgumentNullException(nameof(value));
    }

    [Obsolete("Replace usages of DiffuseRed with ColorRed.")]
    public ResourcePackColorRedChannelProperties? DiffuseRed {
        get => null;
        set => ColorRed = value ?? throw new ArgumentNullException(nameof(value));
    }

    [Obsolete("Replace usages of DiffuseGreen with ColorGreen.")]
    public ResourcePackColorGreenChannelProperties? DiffuseGreen {
        get => null;
        set => ColorGreen = value ?? throw new ArgumentNullException(nameof(value));
    }

    [Obsolete("Replace usages of DiffuseBlue with ColorBlue")]
    public ResourcePackColorBlueChannelProperties? DiffuseBlue {
        get => null;
        set => ColorBlue = value ?? throw new ArgumentNullException(nameof(value));
    }

    [Obsolete("Replace usages of AlbedoRed with ColorRed")]
    public ResourcePackColorRedChannelProperties? AlbedoRed {
        get => null;
        set => ColorRed = value ?? throw new ArgumentNullException(nameof(value));
    }

    [Obsolete("Replace usages of AlbedoGreen with ColorGreen")]
    public ResourcePackColorGreenChannelProperties? AlbedoGreen {
        get => null;
        set => ColorGreen = value ?? throw new ArgumentNullException(nameof(value));
    }

    [Obsolete("Replace usages of AlbedoBlue with ColorBlue")]
    public ResourcePackColorBlueChannelProperties? AlbedoBlue {
        get => null;
        set => ColorBlue = value ?? throw new ArgumentNullException(nameof(value));
    }

    #endregion
}

public class ResourcePackOpacityChannelProperties : PackEncodingChannel
{
    public ResourcePackOpacityChannelProperties() : base(EncodingChannel.Opacity) {}

    public ResourcePackOpacityChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.Opacity, texture, color) {}

    public override object Clone()
    {
        return MemberwiseClone();
    }
}

public class ResourcePackColorRedChannelProperties : PackEncodingChannel
{
    public ResourcePackColorRedChannelProperties() : base(EncodingChannel.ColorRed) {}

    public ResourcePackColorRedChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.ColorRed, texture, color) {}
}

public class ResourcePackColorGreenChannelProperties : PackEncodingChannel
{
    public ResourcePackColorGreenChannelProperties() : base(EncodingChannel.ColorGreen) {}

    public ResourcePackColorGreenChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.ColorGreen, texture, color) {}
}

public class ResourcePackColorBlueChannelProperties : PackEncodingChannel
{
    public ResourcePackColorBlueChannelProperties() : base(EncodingChannel.ColorBlue) {}

    public ResourcePackColorBlueChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.ColorBlue, texture, color) {}
}

public class ResourcePackHeightChannelProperties : PackEncodingChannel
{
    public ResourcePackHeightChannelProperties() : base(EncodingChannel.Height) {}

    public ResourcePackHeightChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.Height, texture, color) {}
}

public class ResourcePackBumpChannelProperties : PackEncodingChannel
{
    public ResourcePackBumpChannelProperties() : base(EncodingChannel.Bump) {}

    public ResourcePackBumpChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.Bump, texture, color) {}
}

public class ResourcePackOcclusionChannelProperties : PackEncodingChannel
{
    public ResourcePackOcclusionChannelProperties() : base(EncodingChannel.Occlusion) {}

    public ResourcePackOcclusionChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.Occlusion, texture, color) {}
}

public class ResourcePackNormalXChannelProperties : PackEncodingChannel
{
    public ResourcePackNormalXChannelProperties() : base(EncodingChannel.NormalX) {}

    public ResourcePackNormalXChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.NormalX, texture, color) {}
}

public class ResourcePackNormalYChannelProperties : PackEncodingChannel
{
    public ResourcePackNormalYChannelProperties() : base(EncodingChannel.NormalY) {}

    public ResourcePackNormalYChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.NormalY, texture, color) {}
}

public class ResourcePackNormalZChannelProperties : PackEncodingChannel
{
    public ResourcePackNormalZChannelProperties() : base(EncodingChannel.NormalZ) {}

    public ResourcePackNormalZChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.NormalZ, texture, color) {}
}

public class ResourcePackSpecularChannelProperties : PackEncodingChannel
{
    public ResourcePackSpecularChannelProperties() : base(EncodingChannel.Specular) {}

    public ResourcePackSpecularChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.Specular, texture, color) {}
}

public class ResourcePackSmoothChannelProperties : PackEncodingChannel
{
    public ResourcePackSmoothChannelProperties() : base(EncodingChannel.Smooth) {}

    public ResourcePackSmoothChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.Smooth, texture, color) {}
}

public class ResourcePackRoughChannelProperties : PackEncodingChannel
{
    public ResourcePackRoughChannelProperties() : base(EncodingChannel.Rough) {}

    public ResourcePackRoughChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.Rough, texture, color) {}
}

public class ResourcePackMetalChannelProperties : PackEncodingChannel
{
    public ResourcePackMetalChannelProperties() : base(EncodingChannel.Metal) {}

    public ResourcePackMetalChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.Metal, texture, color) {}
}

public class ResourcePackHcmChannelProperties : PackEncodingChannel
{
    public ResourcePackHcmChannelProperties() : base(EncodingChannel.HCM) {}

    public ResourcePackHcmChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.HCM, texture, color) {}
}

public class ResourcePackF0ChannelProperties : PackEncodingChannel
{
    public ResourcePackF0ChannelProperties() : base(EncodingChannel.F0) {}

    public ResourcePackF0ChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.F0, texture, color) {}
}

public class ResourcePackPorosityChannelProperties : PackEncodingChannel
{
    public ResourcePackPorosityChannelProperties() : base(EncodingChannel.Porosity) {}

    public ResourcePackPorosityChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.Porosity, texture, color) {}
}

public class ResourcePackSssChannelProperties : PackEncodingChannel
{
    public ResourcePackSssChannelProperties() : base(EncodingChannel.SubSurfaceScattering) {}

    public ResourcePackSssChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.SubSurfaceScattering, texture, color) {}
}

public class ResourcePackEmissiveChannelProperties : PackEncodingChannel
{
    public ResourcePackEmissiveChannelProperties() : base(EncodingChannel.Emissive) {}

    public ResourcePackEmissiveChannelProperties(string texture, ColorChannel color) : base(EncodingChannel.Emissive, texture, color) {}
}