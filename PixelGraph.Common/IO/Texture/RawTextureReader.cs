using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace PixelGraph.Common.IO.Texture
{
    internal class RawTextureReader : TextureReaderBase
    {
        private static readonly Dictionary<string, string> localExp = new(StringComparer.InvariantCultureIgnoreCase) {
            [TextureTags.Opacity] = @"(^|[\s-_.])(opacity|alpha)($|[\s-_.])",
            [TextureTags.Color] = @"(^|[\s-_.])((base)?color|albedo|diffuse)($|[\s-_.])",
            [TextureTags.Height] = @"(^|[\s-_.])height($|[\s-_.])",
            [TextureTags.Bump] = @"(^|[\s-_.])bump($|[\s-_.])",
            [TextureTags.Normal] = @"(^|[\s-_.])normal($|[\s-_.])",
            [TextureTags.Occlusion] = @"(^|[\s-_.])((ambient)?occlusion|ao)($|[\s-_.])",
            [TextureTags.Specular] = @"^specular$",
            [TextureTags.Smooth] = @"(^|[\s-_.])smooth(ness)?($|[\s-_.])",
            [TextureTags.Rough] = @"(^|[\s-_.])rough(ness)?($|[\s-_.])",
            [TextureTags.Metal] = @"(^|[\s-_.])metal(lic|ness)?($|[\s-_.])",
            [TextureTags.HCM] = @"(^|[\s-_.])hcm($|[\s-_.])",
            [TextureTags.F0] = @"(^|[\s-_.])f0($|[\s-_.])",
            [TextureTags.Porosity] = @"(^|[\s-_.])porosity($|[\s-_.])",
            [TextureTags.SubSurfaceScattering] = @"(^|[\s-_.])(sss|scattering)($|[\s-_.])",
            [TextureTags.Emissive] = @"(^|[\s-_.])emissi(ve|on)($|[\s-_.])",
            //[TextureTags.MER] = @"^mer$",
                
            // Deprecated/Internal
            [TextureTags.Item] = @"^(inventory|item)$",
        };

        private static readonly Dictionary<string, string> globalMap = new(StringComparer.InvariantCultureIgnoreCase) {
            [TextureTags.Color] = "color",
            [TextureTags.Opacity] = "opacity",
            [TextureTags.Height] = "height",
            [TextureTags.Bump] = "bump",
            [TextureTags.Normal] = "normal",
            [TextureTags.Occlusion] = "occlusion",
            [TextureTags.Specular] = "specular",
            [TextureTags.Smooth] = "smooth",
            [TextureTags.Rough] = "rough",
            [TextureTags.Metal] = "metal",
            [TextureTags.HCM] = "hcm",
            [TextureTags.F0] = "f0",
            [TextureTags.Porosity] = "porosity",
            [TextureTags.SubSurfaceScattering] = "sss",
            [TextureTags.Emissive] = "emissive",
            //[TextureTags.MER] = name => $"{name}_mer",

            // Internal
            [TextureTags.Item] = "item",
        };


        public RawTextureReader(IServiceProvider provider) : base(provider) {}

        public override bool IsLocalFile(string localFile, string tag)
        {
            if (!localExp.TryGetValue(tag, out var pattern)) return false;

            var fileName = Path.GetFileNameWithoutExtension(localFile);

            if (fileName.EndsWith(".ignore") || fileName.EndsWith("-x")) return false;

            return Regex.IsMatch(fileName, pattern, RegexOptions.IgnoreCase);
        }

        public override bool IsGlobalFile(string localFile, string name, string tag)
        {
            if (!globalMap.TryGetValue(tag, out var globalName)) return false;

            var fileName = Path.GetFileNameWithoutExtension(localFile);
            if (fileName.EndsWith(".ignore") || fileName.EndsWith("-x")) return false;

            return string.Equals(fileName, $"{name}_{globalName}", StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(fileName, globalName, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
