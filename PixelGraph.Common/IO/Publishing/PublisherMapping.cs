using PixelGraph.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace PixelGraph.Common.IO.Publishing;

public interface IPublisherMapping
{
    IDictionary<string, string> Mappings {get;}

    bool Contains(string sourceFile);
    bool TryMap(string sourcePath, string sourceName, out string destPath, out string destName);
}

internal abstract class PublisherMappingBase : IPublisherMapping
{
    private readonly Lazy<IDictionary<string, string>> mappingsLazy;
    //public IDictionary<string, string> Mappings {get; set;}

    public IDictionary<string, string> Mappings => mappingsLazy.Value;


    protected PublisherMappingBase()
    {
        mappingsLazy = new Lazy<IDictionary<string, string>>(OnBuildMappings);
    }

    protected virtual IDictionary<string, string> OnBuildMappings()
    {
        return null; //new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
    }

    public virtual bool Contains(string sourceFile)
    {
        return Mappings.ContainsKey(sourceFile);
    }

    public virtual bool TryMap(string sourcePath, string sourceName, out string destPath, out string destName)
    {
        var sourceFile = PathEx.Join(sourcePath, sourceName);
        sourceFile = PathEx.Normalize(sourceFile);

        if (!Mappings.TryGetValue(sourceFile, out var destFile)) {
            destName = null;
            destPath = null;
            return false;
        }

        destName = Path.GetFileName(destFile);
        destPath = Path.GetDirectoryName(destFile);
        return true;
    }
}