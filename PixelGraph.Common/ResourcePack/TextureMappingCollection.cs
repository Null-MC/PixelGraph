using System;
using PixelGraph.Common.Textures;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using PixelGraph.Common.Material;

namespace PixelGraph.Common.ResourcePack
{
    public class TextureMappingCollection : List<TextureMapping>
    {
        public void Merge(IEnumerable<TextureMapping> source)
        {
            foreach (var sourceMap in source) {
                var destMap = this.FirstOrDefault(m => TextureTags.Is(m.Tag, sourceMap.Tag));

                if (destMap != null) {
                    destMap.Merge(sourceMap);
                }
                else {
                    Add(sourceMap);
                }
            }
        }

        public void Merge(ResourcePackInputProperties input)
        {
            throw new NotImplementedException();
        }

        public void Merge(ResourcePackOutputProperties output)
        {
            throw new NotImplementedException();
        }

        public void Merge(MaterialProperties material)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TextureMapping> GetTexturesWithMappings()
        {
            return this.Where(t => t.HasChannelMappings());
        }

        public bool TryGetChannel(string encodingChannel, out ResourcePackChannelProperties channelProperties)
        {
            channelProperties = collection.FirstOrDefault(c => EncodingChannel.Is(c.ID, encodingChannel));
            return channelProperties != null;
        }
    }
}
