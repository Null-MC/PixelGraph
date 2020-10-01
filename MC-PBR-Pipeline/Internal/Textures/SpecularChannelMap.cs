namespace McPbrPipeline.Internal.Textures
{
    public class SpecularChannelMap
    {
        public ColorChannel Smooth {get; set;} = ColorChannel.Red;
        public ColorChannel Rough {get; set;} = ColorChannel.None;
        public ColorChannel Metal {get; set;} = ColorChannel.Green;
        public ColorChannel Emissive {get; set;} = ColorChannel.Blue;
    }
}
