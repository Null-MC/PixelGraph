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

        private static readonly Dictionary<string, Func<string, string>> globalMap = new(StringComparer.InvariantCultureIgnoreCase) {
            [TextureTags.Color] = name => $"{name}_color",
            [TextureTags.Opacity] = name => $"{name}_opacity",
            [TextureTags.Height] = name => $"{name}_height",
            [TextureTags.Bump] = name => $"{name}_bump",
            [TextureTags.Normal] = name => $"{name}_normal",
            [TextureTags.Occlusion] = name => $"{name}_occlusion",
            [TextureTags.Specular] = name => $"{name}_specular",
            [TextureTags.Smooth] = name => $"{name}_smooth",
            [TextureTags.Rough] = name => $"{name}_rough",
            [TextureTags.Metal] = name => $"{name}_metal",
            [TextureTags.HCM] = name => $"{name}_hcm",
            [TextureTags.F0] = name => $"{name}_f0",
            [TextureTags.Porosity] = name => $"{name}_porosity",
            [TextureTags.SubSurfaceScattering] = name => $"{name}_sss",
            [TextureTags.Emissive] = name => $"{name}_emissive",
            //[TextureTags.MER] = name => $"{name}_mer",

            // Internal
            [TextureTags.Item] = name => $"{name}_item",
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
            if (!globalMap.TryGetValue(tag, out var globalNameFunc)) return false;

            var fileName = Path.GetFileNameWithoutExtension(localFile);

            if (fileName.EndsWith(".ignore") || fileName.EndsWith("-x")) return false;

            var globalName = globalNameFunc(name);
            return string.Equals(fileName, globalName, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
