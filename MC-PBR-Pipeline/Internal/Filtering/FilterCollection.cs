using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;

namespace McPbrPipeline.Internal.Filtering
{
    internal class FilterCollection
    {
        private readonly List<IImageFilter> filterList;

        public string SourceColor {get; set;}

        public bool Empty => filterList.Count == 0;


        public FilterCollection()
        {
            filterList = new List<IImageFilter>();
        }

        public void Append(IImageFilter filter)
        {
            filterList.Add(filter);
        }

        public void Apply(Image image)
        {
            image.Mutate(context => {
                foreach (var filter in filterList)
                    filter.Apply(context);
            });
        }
    }
}
