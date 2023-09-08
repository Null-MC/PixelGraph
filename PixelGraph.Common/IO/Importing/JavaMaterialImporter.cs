using PixelGraph.Common.Extensions;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace PixelGraph.Common.IO.Importing;

internal class JavaMaterialImporter : MaterialImporterBase
{
    private static readonly Regex isPathMaterialExp = new(@"^assets/[\w-]+/textures/(?:block|item|entity|models|painting)/?", RegexOptions.IgnoreCase);


    public JavaMaterialImporter(IServiceProvider provider) : base(provider) {}

    public override bool IsMaterialFile(string filename, out string name)
    {
        name = Path.GetFileNameWithoutExtension(filename);

        // TODO: get all input names
        //foreach (var tag in context.OutputEncoding.Select()) {
        //    var tagName = texWriter.Get(name, tag);
        //}

        var isNormal = name.EndsWith("_n", StringComparison.InvariantCultureIgnoreCase);
        var isSpecular = name.EndsWith("_s", StringComparison.InvariantCultureIgnoreCase);
        var isEmissive = name.EndsWith("_e", StringComparison.InvariantCultureIgnoreCase);

        if (isNormal || isSpecular || isEmissive) {
            name = name[..^2];
            return true;
        }

        var path = Path.GetDirectoryName(filename);
        path = PathEx.Normalize(path);
        if (path == null) return false;

        return isPathMaterialExp.IsMatch(path);
    }
}