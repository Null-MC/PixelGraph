using PixelGraph.Common.Material;
using PixelGraph.Common.TextureFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PixelGraph.Common.ResourcePack
{
    public class TextureMapping
    {
        public Func<ResourcePackProfileProperties, MaterialProperties, string> Name {get; set;}
        public List<ChannelMapping> Channels {get; set;}
        public string Tag {get; set;}


        public virtual bool HasAnyData()
        {
            if (Name != null) return true;
            if (Channels is {Count: > 0}) return true;
            return false;
        }

        public bool HasChannelMappings()
        {
            return Channels.Any(c => c.HasMapping);
        }

        //public virtual object Clone()
        //{
        //    return MemberwiseClone();
        //}

        public void Merge(TextureMapping sourceMap)
        {
            foreach (var sourceChannel in sourceMap.Channels) {
                var destChannel = Channels.FirstOrDefault(c => EncodingChannel.Is(c.Type, sourceChannel.Type));

                if (destChannel != null) {
                    destChannel.Merge(sourceChannel);
                }
                else {
                    Channels.Add(sourceChannel);
                }
            }
        }
    }
}
