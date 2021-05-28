using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PixelGraph.Common.IO.Publishing
{
    internal class BedrockPackMetadata
    {
        [JsonProperty("format_version")]
        public int FormatVersion {get; set;}

        [JsonProperty("header")]
        public BedrockPackHeaderMetadata Header {get; set;}

        [JsonProperty("modules")]
        public List<BedrockPackModuleMetadata> Modules {get; set;}

        [JsonProperty("capabilities")]
        public List<string> Capabilities {get; set;}


        public BedrockPackMetadata()
        {
            Header = new BedrockPackHeaderMetadata();
            Modules = new List<BedrockPackModuleMetadata>();
            Capabilities = new List<string>();
        }
    }

    internal class BedrockPackHeaderMetadata
    {
        [JsonProperty("name")]
        public string Name {get; set;}

        [JsonProperty("description")]
        public string Description {get; set;}

        [JsonProperty("uuid")]
        public Guid? UniqueId {get; set;}

        [JsonProperty("version")]
        public int[] Version {get; set;}

        [JsonProperty("min_engine_version")]
        public int[] MinEngineVersion {get; set;}


        //public BedrockPackHeaderMetadata()
        //{
        //    //
        //}
    }

    internal class BedrockPackModuleMetadata
    {
        [JsonProperty("description")]
        public string Description {get; set;}

        [JsonProperty("type")]
        public string Type {get; set;}

        [JsonProperty("uuid")]
        public Guid? UniqueId {get; set;}

        [JsonProperty("version")]
        public int[] Version {get; set;}
    }
}
