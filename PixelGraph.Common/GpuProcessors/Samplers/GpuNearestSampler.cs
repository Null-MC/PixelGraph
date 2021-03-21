using ComputeSharp;

namespace PixelGraph.Common.GpuProcessors.Samplers
{
    internal struct GpuNearestSampler<T> : IGpuSampler<T>
        where T : unmanaged
    {
        public ReadOnlyBuffer<T> Buffer {get; set;}
        public int BufferWidth {get; set;}
        public int BufferHeight {get; set;}
        public bool WrapX {get; set;}
        public bool WrapY {get; set;}


        public void Sample(in float x, in float y, out T value)
        {
            var px = (int)Hlsl.Round(x);
            this.WrapClampX(ref px);

            var py = (int)Hlsl.Round(y);
            this.WrapClampY(ref py);

            var i = px * BufferWidth + py;
            value = Buffer[i];
        }
    }
}
