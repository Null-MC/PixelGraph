namespace PixelGraph.Common.ResourcePack
{
    public class ResourcePackInputProperties : ResourcePackEncoding
    {
        public string Format {get; set;}
        //public string Sampler {get; set;}


        public override object Clone()
        {
            var clone = (ResourcePackInputProperties)base.Clone();

            clone.Format = Format;
            //clone.Sampler = Sampler;

            //clone.Alpha = (ResourcePackAlphaChannelProperties)Alpha.Clone();

            //clone.DiffuseRed = (ResourcePackDiffuseRedChannelProperties)DiffuseRed.Clone();
            //clone.DiffuseGreen = (ResourcePackDiffuseGreenChannelProperties)DiffuseGreen.Clone();
            //clone.DiffuseBlue = (ResourcePackDiffuseBlueChannelProperties)DiffuseBlue.Clone();

            //clone.AlbedoRed = (ResourcePackAlbedoRedChannelProperties)AlbedoRed.Clone();
            //clone.AlbedoGreen = (ResourcePackAlbedoGreenChannelProperties)AlbedoGreen.Clone();
            //clone.AlbedoBlue = (ResourcePackAlbedoBlueChannelProperties)AlbedoBlue.Clone();

            //clone.Height = (ResourcePackHeightChannelProperties)Height.Clone();
            //clone.Occlusion = (ResourcePackOcclusionChannelProperties)Occlusion.Clone();

            //clone.NormalX = (ResourcePackNormalXChannelProperties)NormalX.Clone();
            //clone.NormalY = (ResourcePackNormalYChannelProperties)NormalY.Clone();
            //clone.NormalZ = (ResourcePackNormalZChannelProperties)NormalZ.Clone();

            //clone.Specular = (ResourcePackSpecularChannelProperties)Specular.Clone();

            //clone.Smooth = (ResourcePackSmoothChannelProperties)Smooth.Clone();
            //clone.Rough = (ResourcePackRoughChannelProperties)Rough.Clone();

            //clone.Metal = (ResourcePackMetalChannelProperties)Metal.Clone();

            //clone.Porosity = (ResourcePackPorosityChannelProperties)Porosity.Clone();

            //clone.SSS = (ResourcePackSssChannelProperties)SSS.Clone();

            //clone.Emissive = (ResourcePackEmissiveChannelProperties)Emissive.Clone();

            return clone;
        }
    }
}
