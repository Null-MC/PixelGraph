﻿using McPbrPipeline.Internal.Textures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace McPbrPipeline.Internal
{
    public interface IProfile
    {
        string Source {get;}
        DateTime ProfileWriteTime {get;}
        int PackFormat {get;}
        string Description {get;}
        List<string> Tags {get;}
        string ResizeSampler {get;}
        float? TextureScale {get;}
        int? TextureSize {get;}
        bool? IncludeAlbedo {get;}
        bool? IncludeNormal {get;}
        bool? IncludeSpecular {get;}
        bool? IncludeEmissive {get;}
        SpecularChannelMap SpecularIn {get;}
        SpecularChannelMap SpecularOut {get;}


        string GetSourcePath(params string[] path) => GetPath(Source, path);

        bool SpecularChannelsMatch()
        {
            if (SpecularOut == null) return true;

            return SpecularIn.Smooth == SpecularOut.Smooth &&
                SpecularIn.Rough == SpecularOut.Rough &&
                SpecularIn.Metal == SpecularOut.Metal &&
                SpecularIn.Emissive == SpecularOut.Emissive;
        }

        private static string GetPath(string basePath, string[] localPath)
        {
            return localPath != null
                ? Path.GetFullPath(Path.Combine(localPath), basePath)
                : Path.GetFullPath(basePath);
        }
    }

    internal class Profile : IProfile
    {
        [JsonIgnore]
        public string Source {get; set;}

        [JsonIgnore]
        public DateTime ProfileWriteTime {get; set;}

        public int PackFormat {get; set;}
        public string Description {get; set;}
        public List<string> Tags {get; set;}

        [JsonProperty("resize-sampler")]
        public string ResizeSampler {get; set;}

        [JsonProperty("texture-scale")]
        public float? TextureScale {get; set;}

        [JsonProperty("texture-size")]
        public int? TextureSize {get; set;}

        [JsonProperty("include-albedo")]
        public bool? IncludeAlbedo {get; set;}

        [JsonProperty("include-normal")]
        public bool? IncludeNormal {get; set;}

        [JsonProperty("include-specular")]
        public bool? IncludeSpecular {get; set;}

        [JsonProperty("include-emissive")]
        public bool? IncludeEmissive {get; set;}

        [JsonProperty("specular-in")]
        public SpecularChannelMap SpecularIn {get; set;}

        [JsonProperty("specular-out")]
        public SpecularChannelMap SpecularOut {get; set;}


        public Profile()
        {
            PackFormat = 5;
            Tags = new List<string>();
            SpecularIn = new SpecularChannelMap();
        }
    }
}