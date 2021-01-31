namespace PixelGraph.Common.ResourcePack
{
    public class ResourcePackInputProperties : ResourcePackEncoding
    {
        public const bool AutoMaterialDefault = true;

        public string Format {get; set;}
        public bool? AutoMaterial {get; set;}


        public override object Clone()
        {
            var clone = (ResourcePackInputProperties)base.Clone();

            clone.Format = Format;

            return clone;
        }
    }
}
