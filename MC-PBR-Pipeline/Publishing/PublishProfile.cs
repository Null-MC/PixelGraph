using System.Collections.Generic;
using System.IO;

namespace McPbrPipeline.Publishing
{
    internal class PublishProfile
    {
        public int PackFormat {get; set;}
        public string Description {get; set;}
        public List<string> Tags {get; set;}
        public string Source {get; set;}
        public string Destination {get; set;}
        public int? TextureWidth {get; set;}
        public int? TextureHeight {get; set;}

        public bool HasTextureDimensions => TextureWidth.HasValue && TextureHeight.HasValue;


        public PublishProfile()
        {
            PackFormat = 5;
            Source = ".";
            Destination = Path.Combine(".", "published");
            Tags = new List<string>();
        }
    }
}
