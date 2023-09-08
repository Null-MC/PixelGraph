using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO.Importing;

public interface IResourcePackImporter
{
    bool AsGlobal {get; set;}
    bool CopyUntracked {get; set;}
    bool IncludeUnknown {get; set;}
    IProjectDescription Project {get; set;}
    PublishProfileProperties PackProfile {get; set;}

    Task ImportAsync(CancellationToken token = default);
}

public class ResourcePackImporter : IResourcePackImporter
{
    private readonly ILogger<ResourcePackImporter> logger;
    private readonly IInputReader reader;
    private readonly IOutputWriter writer;
    private readonly IMaterialImporter importer;
    //private readonly IMaterialWriter matWriter;

    public bool AsGlobal {get; set;}
    public bool CopyUntracked {get; set;}
    public bool IncludeUnknown {get; set;}
    public IProjectDescription Project {get; set;}
    public PublishProfileProperties PackProfile {get; set;}

    public int FailureCount {get; set;}


    //static ResourcePackImporter()
    //{
    //    ctmPropertySerializer = new ObjectPropertyFileSerializer<CtmProperties>();
    //}

    public ResourcePackImporter(IServiceProvider provider)
    {
        reader = provider.GetRequiredService<IInputReader>();
        writer = provider.GetRequiredService<IOutputWriter>();
        importer = provider.GetRequiredService<IMaterialImporter>();
        //matWriter = provider.GetRequiredService<IMaterialWriter>();
        logger = provider.GetRequiredService<ILogger<ResourcePackImporter>>();
    }

    public async Task ImportAsync(CancellationToken token = default)
    {
        FailureCount = 0;
        await ImportPathAsync(".", token);

        if (FailureCount > 0) logger.LogWarning("Failed to import {FailureCount:N0} materials!", FailureCount);
    }

    private async Task ImportPathAsync(string localPath, CancellationToken token)
    {
        foreach (var childPath in reader.EnumerateDirectories(localPath)) {
            if (!IncludeUnknown && IsUnknownPath(childPath)) continue;

            await ImportPathAsync(childPath, token);
        }

        var fileList = reader.EnumerateFiles(localPath);
        if (!IncludeUnknown) fileList = fileList.Where(f => !IsUnknownFile(f));

        var files = fileList.ToHashSet(StringComparer.InvariantCultureIgnoreCase);

        files.Remove("manifest.json");

        // TODO: detect and remove CTM files first
        //if (ctmExp.IsMatch(localPath)) {
        //    foreach (var file in files) {
        //        var ext = Path.GetExtension(file);
        //        if (!ext.Equals(".properties", StringComparison.InvariantCultureIgnoreCase)) continue;
        //        var name = Path.GetFileNameWithoutExtension(file);

        //        // parse ctm properties file
        //        await using var stream = reader.Open(file);
        //        using var streamReader = new StreamReader(stream);
        //        var ctmProperties = await ctmPropertySerializer.ReadAsync(streamReader, token);

        //        // only supporting repeat-ctm for now
        //        if (!CtmTypes.Is(ctmProperties.Method, CtmTypes.Repeat)) continue;

        //        // Build 1D tile array
        //        var ctmWidth = ctmProperties.Width ?? 1;
        //        var ctmHeight = ctmProperties.Height ?? 1;
        //        var expectedLength = ctmWidth * ctmHeight;

        //        var tileFiles = ParseCtmTiles(ctmProperties, localPath).ToArray();
        //        if (expectedLength < 1) throw new ApplicationException($"Invalid ctm dimensions! expected count={expectedLength}");
        //        if (tileFiles.Length != expectedLength) throw new ApplicationException($"Expected {expectedLength:N0} ctm tiles but found {tileFiles.Length:N0}!");

        //        // Build 2D tile array
        //        var tileMap = new string[ctmWidth, ctmHeight];
        //        for (var y = 0; y < ctmHeight; y++) {
        //            for (var x = 0; x < ctmWidth; x++) {
        //                var f = tileFiles[y * ctmWidth + x];
        //                tileMap[x, y] = f;

        //                // TODO: remove f from files (if same folder)
        //            }
        //        }

        //        importer.AsGlobal = AsGlobal;
        //        importer.PackInput = PackInput;
        //        importer.PackProfile = PackProfile;

        //        // TODO: BuildMaterial() with ctm tileMap
        //        //...

        //        // TODO: create single material file
        //        var material = await importer.CreateMaterialAsync(localPath, name);
        //    }
        //}

        var names = GetMaterialNames(files).Distinct().ToArray();

        foreach (var name in names) {
            token.ThrowIfCancellationRequested();

            var file = PathEx.Join(localPath, name);
            file = PathEx.Normalize(file);

            try {
                await ImportMaterialAsync(localPath, name, token);
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to import material '{file}'!", file);
                FailureCount++;
            }

            RemoveNamedFile(files, file);

            // Remove from untracked files
            // WARN: This only works for default file names!
            // TODO: use mapped file names
            //if (IsBedrock) {
            RemoveNamedFile(files, $"{file}_n");
            RemoveNamedFile(files, $"{file}_s");
            //RemoveFile(files, $"{name}_e");
            //}
            //else {
            files.Remove($"{file}.texture_set.json");
            RemoveNamedFile(files, $"{file}_heightmap");
            RemoveNamedFile(files, $"{file}_normal");
            RemoveNamedFile(files, $"{file}_mer");
            //}
        }

        if (!CopyUntracked) return;

        foreach (var file in files) {
            token.ThrowIfCancellationRequested();

            await CopyFileAsync(file, token);
        }
    }

    //private IEnumerable<string> ParseCtmTiles(CtmProperties properties, string localPath)
    //{
    //    var parts = properties.Tiles?.Trim().Split(new [] {' ', '\t'});
    //    if (parts == null || parts.Length == 0) yield break;

    //    foreach (var part in parts) {
    //        if (TryParseRange(part, out var rangeMin, out var rangeMax)) {
    //            foreach (var i in Enumerable.Range(rangeMin, rangeMax - rangeMin + 1))
    //                yield return GetTileFilename(localPath, i.ToString());
    //        }
    //        else {
    //            yield return GetTileFilename(localPath, part);
    //        }
    //    }
    //}

    //private string GetTileFilename(string localPath, string part)
    //{
    //    var partPath = Path.GetDirectoryName(part);

    //    if (partPath == null) {
    //        // TODO: scan local folder

    //    }
    //    else {
    //        // TODO: scan relative folder
    //    }

    //    throw new ApplicationException("Unable to locate tile");
    //}

    private async Task<MaterialProperties> ImportMaterialAsync(string localPath, string name, CancellationToken token)
    {
        importer.AsGlobal = AsGlobal;
        importer.Project = Project;
        importer.PackProfile = PackProfile;

        var material = new MaterialProperties {
            Name = name,
            LocalPath = localPath,
            UseGlobalMatching = AsGlobal,
        };

        await importer.ImportAsync(material, token);
        return material;
    }

    private async Task CopyFileAsync(string file, CancellationToken token)
    {
        await using var sourceStream = reader.Open(file);
        if (sourceStream == null) throw new ApplicationException($"Failed to open file '{file}'!");

        await writer.OpenWriteAsync(file, async destStream => {
            await sourceStream.CopyToAsync(destStream, token);
        }, token);
    }

    private static void RemoveNamedFile(HashSet<string> files, string name)
    {
        var matchPath = Path.GetDirectoryName(name);
        var matchName = Path.GetFileName(name);

        files.RemoveWhere(f => {
            var fPath = Path.GetDirectoryName(f);
            if (!string.Equals(fPath, matchPath, StringComparison.InvariantCultureIgnoreCase)) return false;

            var fName = Path.GetFileNameWithoutExtension(f);
            return string.Equals(fName, matchName, StringComparison.InvariantCultureIgnoreCase);
        });
    }

    private IEnumerable<string> GetMaterialNames(IEnumerable<string> files)
    {
        foreach (var file in files) {
            var ext = Path.GetExtension(file);
            if (!ImageExtensions.Supports(ext)) continue;

            if (importer.IsMaterialFile(file, out var name))
                yield return name;
        }
    }

    public static bool IsUnknownPath(string path)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));
        var name = Path.GetFileName(path);

        if (name.Equals(".git", StringComparison.InvariantCultureIgnoreCase)) return true;
        if (name.Equals("META-INF", StringComparison.InvariantCultureIgnoreCase)) return true;
        if (name.EndsWith(".ignore", StringComparison.InvariantCultureIgnoreCase)) return true;

        // Ignore root minecraft.jar directories
        var subPath = PathEx.Normalize(Path.GetDirectoryName(path));
        if (subPath?.StartsWith("com/mojang") ?? false) return true;
        if (subPath?.StartsWith("net/minecraft") ?? false) return true;
        if (subPath?.StartsWith("data/minecraft") ?? false) return true;

        return false;
    }

    public static bool IsUnknownFile(string filename)
    {
        var ext = Path.GetExtension(filename);
        if (ImageExtensions.Supports(ext)) return false;

        if (ext.Equals(".json", StringComparison.InvariantCultureIgnoreCase)) return false;
        if (ext.Equals(".mcmeta", StringComparison.InvariantCultureIgnoreCase)) return false;
        if (ext.Equals(".properties", StringComparison.InvariantCultureIgnoreCase)) return false;

        return true;
    }

    //private static bool TryParseRange(string part, out int min, out int max)
    //{
    //    var separator = part.IndexOf('-');

    //    if (separator < 0) {
    //        min = max = 0;
    //        return false;
    //    }

    //    try {
    //        min = int.Parse(part[..separator]);
    //        max = int.Parse(part[(separator + 1)..]);
    //        return true;
    //    }
    //    catch {
    //        min = max = 0;
    //        return false;
    //    }
    //}
}