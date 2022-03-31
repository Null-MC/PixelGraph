using YamlDotNet.Serialization;

namespace PixelGraph.Common.ResourcePack
{
    public class ResourcePackInputProperties : ResourcePackEncoding
    {
        public const bool AutoMaterialDefault = false;

        /// <summary>
        /// Gets or sets the named texture encoding format for reading image data.
        /// </summary>
        /// <remarks>
        /// See <see cref="PixelGraph.Common.TextureFormats.TextureFormat"/>.
        /// </remarks>
        public string Format {get; set;}

        public bool? AutoMaterial {get; set;}

        /// <summary>
        /// Gets or sets the edition of Minecraft the RP will target.
        /// </summary>
        /// <remarks>
        /// See <see cref="PixelGraph.Common.IO.GameEdition"/>.
        /// </remarks>
        [YamlMember(Order = -99)]
        public string Edition {get; set;}


        public override object Clone()
        {
            var clone = (ResourcePackInputProperties)base.Clone();

            //clone.Format = Format;

            return clone;
        }
    }
}
