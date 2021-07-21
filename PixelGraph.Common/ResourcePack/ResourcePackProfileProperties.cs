using System;
using YamlDotNet.Serialization;

namespace PixelGraph.Common.ResourcePack
{
    public class ResourcePackProfileProperties
    {
        public const int DefaultJavaFormat = 6;
        public const int DefaultBedrockFormat = 2;
        public const decimal DefaultDiffuseOcclusionStrength = 1.0m;
        public const decimal DefaultOcclusionQuality = 0.06m;
        public const decimal DefaultOcclusionPower = 1.0m;
        public const bool AutoGenerateNormalDefault = true;
        public const bool AutoGenerateOcclusionDefault = false;
        public const bool BakeOcclusionToColorDefault = false;

        [YamlIgnore]
        public string LocalFile {get; set;}

        /// <summary>
        /// Gets or sets the edition of Minecraft the RP will target.
        /// </summary>
        /// <remarks>
        /// Supports Java and Bedrock.
        /// </remarks>
        [YamlMember(Order = -99)]
        public string Edition {get; set;}

        /// <summary>
        /// Gets or sets the Header UUID.
        /// For Bedrock only!
        /// </summary>
        [YamlMember(Order = -98)]
        public Guid? HeaderUuid {get; set;}

        /// <summary>
        /// Gets or sets the Module UUID.
        /// For Bedrock only!
        /// </summary>
        [YamlMember(Order = -97)]
        public Guid? ModuleUuid {get; set;}

        /// <summary>
        /// Gets or sets the revision of the RP formatting.
        /// </summary>
        /// <remarks>
        ///   1: 1.6.1 – 1.8.9,
        ///   2: 1.9 – 1.10.2,
        ///   3: 1.11 – 1.12.2,
        ///   4: 1.13 – 1.14.4,
        ///   5: 1.15 – 1.16.1,
        ///   6: 1.16.2 – 1.16.5,
        ///   7: 1.17+
        /// </remarks>
        [YamlMember(Order = -97)]
        public int? Format {get; set;}

        /// <summary>
        /// Gets or sets the name that will be shown next to the RP in-game.
        /// For Bedrock only!
        /// </summary>
        public string Name {get; set;}

        /// <summary>
        /// Gets or sets the text description that will be shown next to the RP in-game.
        /// </summary>
        public string Description {get; set;}

        /// <summary>
        /// Gets or sets the tag text that will be shown next to the RP in-game.
        /// </summary>
        /// <remarks>
        /// This is the second line of the RP description in-game.
        /// </remarks>
        public string Tags {get; set;}

        public ResourcePackOutputProperties Encoding {get; set;}

        public int? TextureSize {get; set;}
        public int? BlockTextureSize {get; set;}
        public decimal? TextureScale {get; set;}

        public decimal? DiffuseOcclusionStrength {get; set;}

        public bool? AutoGenerateNormal {get; set;}

        public bool? AutoGenerateOcclusion {get; set;}

        public bool? BakeOcclusionToColor {get; set;}

        public decimal? OcclusionQuality {get; set;}

        public decimal? OcclusionPower {get; set;}


        public ResourcePackProfileProperties()
        {
            Encoding = new ResourcePackOutputProperties();
        }

        //public virtual object Clone()
        //{
        //    var clone = (ResourcePackProfileProperties)MemberwiseClone();
        //    clone.Encoding = (ResourcePackOutputProperties)Encoding.Clone();
        //    return clone;
        //}
    }
}
