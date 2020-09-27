using System;
using System.Collections.Generic;
using System.Text;
using McPbrPipeline.Filters;

namespace McPbrPipeline.Publishing
{
    internal class TexturePublisher
    {
        public void Publish(PublishProfile profile)
        {
            var filters = new FilterCollection();

            if (profile.HasTextureDimensions) {
                filters.Append(new ResizeFilter {
                    TargetWidth = profile.TextureWidth ?? 0,
                    TargetHeight = profile.TextureHeight ?? 0,
                });
            }
        }
    }
}
