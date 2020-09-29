namespace McPbrPipeline.Filters
{
    internal struct ScaleOptions
    {
        public float? Red;
        public float? Green;
        public float? Blue;
        public float? Alpha;


        public ScaleOptions(float red = 1f, float green = 1f, float blue = 1f, float alpha = 1f)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }
    }
}
