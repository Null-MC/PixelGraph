using System.Collections.Generic;
using System.IO;

namespace McPbrPipeline.Publishing
{
    public interface IPublishProfile
    {
        int PackFormat {get;}
        string Description {get;}
        List<string> Tags {get;}
        string Source {get;}
        string Destination {get;}
        int? TextureSize {get;}


        string GetSourcePath(params string[] path) => GetPath(Source, path);
        string GetDestinationPath(params string[] path) => GetPath(Destination, path);

        private static string GetPath(string basePath, string[] localPath)
        {
            if (localPath == null) return Path.GetFullPath(basePath);
            return Path.GetFullPath(Path.Combine(localPath), basePath);
        }
    }

    internal class PublishProfile : IPublishProfile
    {
        public int PackFormat {get; set;}
        public string Description {get; set;}
        public List<string> Tags {get; set;}
        public string Source {get; set;}
        public string Destination {get; set;}
        public int? TextureSize {get; set;}
        public int? TextureHeight {get; set;}


        public PublishProfile()
        {
            PackFormat = 5;
            Source = ".";
            Destination = Path.Combine(".", "published");
            Tags = new List<string>();
        }
    }
}
