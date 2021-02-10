using YamlDotNet.Serialization;

namespace PixelGraph.Common.ResourcePack
{
    public class ResourcePackProfileProperties
    {
        public const int DefaultFormat = 6;
        public const bool AutoGenerateNormalDefault = true;
        public const bool AutoGenerateOcclusionDefault = true;

        [YamlIgnore]
        public string LocalFile {get; set;}

        /// <summary>
        /// Gets or sets the edition of Minecraft the RP will target.
        /// </summary>
        /// <remarks>
        /// Supports Java and Bedrock.
        /// </remarks>
        [YamlMember(Order = 1)]
        public string Edition {get; set;}

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
        [YamlMember(Order = 0)]
        public int? Format {get; set;}

        /// <summary>
        /// Gets or sets the text description that will be shown next to the RP in-game.
        /// </summary>
        [YamlMember(Order = 0)]
        public string Description {get; set;}

        /// <summary>
        /// Gets or sets the tag text that will be shown next to the RP in-game.
        /// </summary>
        /// <remarks>
        /// This is the second line of the RP description in-game.
        /// </remarks>
        public string Tags {get; set;}

        public ResourcePackOutputProperties Encoding {get; set;}

        /// <summary>
        /// Gets or sets the size to use when creating a new texture
        /// and no existing resolution can be identified.
        /// </summary>
        public int? DefaultTextureSize {get; set;}

        public int? TextureSize {get; set;}
        public float? TextureScale {get; set;}

        public int? BlockTextureSize {get; set;}
        public float? BlockTextureScale {get; set;}

        public int? EntityTextureSize {get; set;}
        public float? EntityTextureScale {get; set;}

        //public int? BlockSize {get; set;}

        public bool? AutoGenerateNormal {get; set;}

        public bool? AutoGenerateOcclusion {get; set;}


        public ResourcePackProfileProperties()
        {
            Encoding = new ResourcePackOutputProperties();
        }
    }
}
