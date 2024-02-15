using MahApps.Metro.IconPacks;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.UI.ViewModels;
using System.IO;

namespace PixelGraph.UI.Internal.IO;

internal class ContentTreeReader
{
    private readonly IInputReader reader;
    private readonly IPublishReader loader;


    public ContentTreeReader(
        IInputReader reader,
        IPublishReader loader)
    {
        this.reader = reader;
        this.loader = loader;
    }

    public void Update(ContentTreeNode parentNode)
    {
        var existingNodes = parentNode.Nodes.ToList();

        foreach (var childPath in reader.EnumerateDirectories(parentNode.LocalPath)) {
            var isMat = IsLocalMaterialPath(childPath, out var matFile);

            var existingNode = TryRemove(existingNodes, x => {
                if (isMat && x is not ContentTreeMaterialDirectory) return false;
                if (!isMat && x is not ContentTreeDirectory) return false;
                return string.Equals(x.LocalPath, childPath, StringComparison.InvariantCultureIgnoreCase);
            });

            // Add new folder nodes
            if (existingNode == null) {
                existingNode = isMat
                    ? new ContentTreeMaterialDirectory(parentNode) {
                        MaterialFilename = matFile,
                    }
                    : new ContentTreeDirectory(parentNode);

                existingNode.Name = Path.GetFileName(childPath);
                existingNode.LocalPath = childPath;
                parentNode.Nodes.Add(existingNode);
            }

            Update(existingNode);
        }

        var isParentMat = parentNode is ContentTreeMaterialDirectory;
        foreach (var file in reader.EnumerateFiles(parentNode.LocalPath)) {
            var fileName = Path.GetFileName(file);
            if (isParentMat && string.Equals(fileName, "mat.yml", StringComparison.InvariantCultureIgnoreCase)) continue;
            if (isParentMat && string.Equals(fileName, "pbr.yml", StringComparison.InvariantCultureIgnoreCase)) continue;

            var existingNode = TryRemove(existingNodes, x => {
                if (x is not ContentTreeFile fileNode) return false;
                return string.Equals(fileNode.Filename, file, StringComparison.InvariantCultureIgnoreCase);
            });

            // Add new file nodes
            if (existingNode == null) {
                existingNode = new ContentTreeFile(parentNode) {
                    Name = fileName,
                    Filename = file,
                    Type = GetNodeType(fileName),
                    Icon = GetNodeIcon(fileName),
                };

                parentNode.Nodes.Add(existingNode);
            }
        }

        // Remove nodes that no longer exist
        foreach (var node in existingNodes)
            parentNode.Nodes.Remove(node);
    }

    private static ContentTreeNode? TryRemove(IList<ContentTreeNode> nodes, Func<ContentTreeNode, bool> filter)
    {
        for (var i = nodes.Count - 1; i >= 0; i--) {
            var node = nodes[i];
            if (!filter(node)) continue;
                    
            nodes.RemoveAt(i);
            return node;
        }

        return null;
    }
        
    private bool IsLocalMaterialPath(string localPath, out string filename)
    {
        filename = PathEx.Join(localPath, "mat.yml");
        if (reader.FileExists(filename)) return true;

        filename = PathEx.Join(localPath, "pbr.yml");
        if (reader.FileExists(filename)) return true;

        if (loader.EnableAutoMaterial && loader.IsLocalMaterialPath(localPath)) {
            filename = PathEx.Join(localPath, "mat.yml");
            return true;
        }

        return false;
    }

    private static ContentNodeType GetNodeType(string fileName)
    {
        if (string.Equals("input.yml", fileName, StringComparison.InvariantCultureIgnoreCase))
            return ContentNodeType.PackInput;

        if (fileName.EndsWith(".pack.yml", StringComparison.InvariantCultureIgnoreCase))
            return ContentNodeType.PackProfile;

        if (string.Equals("mat.yml", fileName, StringComparison.InvariantCultureIgnoreCase)
            || fileName.EndsWith(".mat.yml", StringComparison.InvariantCultureIgnoreCase))
            return ContentNodeType.Material;

        if (string.Equals("pbr.yml", fileName, StringComparison.InvariantCultureIgnoreCase)
            || fileName.EndsWith(".pbr.yml", StringComparison.InvariantCultureIgnoreCase))
            return ContentNodeType.Material;

        var ext = Path.GetExtension(fileName);
        if (ImageExtensions.Supports(ext)) return ContentNodeType.Texture;

        return ContentNodeType.Unknown;
    }

    private static PackIconFontAwesomeKind GetNodeIcon(string fileName)
    {
        if (string.Equals("input.yml", fileName, StringComparison.InvariantCultureIgnoreCase))
            return PackIconFontAwesomeKind.PaletteSolid;

        if (fileName.EndsWith(".pack.yml", StringComparison.InvariantCultureIgnoreCase))
            return PackIconFontAwesomeKind.FileExportSolid;

        if (string.Equals("mat.yml", fileName, StringComparison.InvariantCultureIgnoreCase)
            || fileName.EndsWith(".mat.yml", StringComparison.InvariantCultureIgnoreCase))
            return PackIconFontAwesomeKind.BrushSolid;

        if (string.Equals("pbr.yml", fileName, StringComparison.InvariantCultureIgnoreCase)
            || fileName.EndsWith(".pbr.yml", StringComparison.InvariantCultureIgnoreCase))
            return PackIconFontAwesomeKind.BrushSolid;

        var ext = Path.GetExtension(fileName);
        if (ImageExtensions.Supports(ext)) return PackIconFontAwesomeKind.ImageSolid;

        return PackIconFontAwesomeKind.FileSolid;
    }
}