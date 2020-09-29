namespace McPbrPipeline.Filters
{
    internal class NormalMapOptions
    {
        public int DownSample {get; set;} = 1;
        public float Strength {get; set;} = 1f;
        public float Blur {get; set;} = 0f;
        public bool Wrap {get; set;} = true;
    }
}
