using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace McPbrPipeline.Internal.Publishing
{
    public interface IPublishProfile
    {
        int PackFormat {get;}
        string Description {get;}
        List<string> Tags {get;}
        string Source {get;}
        string Destination {get;}
        int? TextureSize {get;}

        [JsonProperty("include-normal")]
        bool? IncludeNormal {get;}

        [JsonProperty("include-specular")]
        bool? IncludeSpecular {get;}


        string GetSourcePath(params string[] path) => GetPath(Source, path);
        string GetDestinationPath(params string[] path) => GetPath(Destination, path);

        private static string GetPath(string basePath, string[] localPath)
        {
            return localPath != null
                ? Path.GetFullPath(Path.Combine(localPath), basePath)
                : Path.GetFullPath(basePath);
        }
    }

    internal class PublishProfile : IPublishProfile
    {
        public int PackFormat {get; set;}
        public string Description {get; set;}
        public List<string> Tags {get; set;}
        public string Source {get; set;}
        public string Destination {get; set;}
        public bool? IncludeNormal {get; set;}
        public bool? IncludeSpecular {get; set;}

        [JsonProperty("texture-size")]
        public int? TextureSize {get; set;}


        public PublishProfile()
        {
            PackFormat = 5;
            //Source = ".";
            //Destination = Path.Combine(".", "published");
            Tags = new List<string>();
        }
    }
}
