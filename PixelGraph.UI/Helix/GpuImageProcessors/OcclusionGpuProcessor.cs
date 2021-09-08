using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PixelGraph.UI.Helix.GpuImageProcessors
{
    internal class OcclusionGpuProcessor
    {
        private readonly ITextureRegionEnumerator regions;


        public OcclusionGpuProcessor(ITextureRegionEnumerator regions)
        {
            this.regions = regions;
        }

        public Image<L8> Generate(int frameCount)
        {
            // TODO: extract raw height map

            var finalImage = new Image<L8>(Configuration.Default, 1, 1);

            try {
                foreach (var region in regions.GetAllRenderRegions(null, frameCount)) {
                    // TODO: extract tile region

                    // TODO: popular gpu buffers

                    // TODO: apply shader

                    // TODO: copy to final image
                }

                return finalImage;
            }
            catch {
                finalImage.Dispose();
                throw;
            }
        }
    }
}
