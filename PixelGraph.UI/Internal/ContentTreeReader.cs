using MaterialDesignThemes.Wpf;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.UI.ViewModels;
using System;
using System.IO;

namespace PixelGraph.UI.Internal
{
    internal interface IContentTreeReader
    {
        ContentTreeNode GetRootNode();
        ContentTreeNode GetPathNode(ContentTreeNode parent, string localPath);
    }

    internal class ContentTreeReader : IContentTreeReader
    {
        private readonly IInputReader reader;
        private readonly IFileLoader loader;


        public ContentTreeReader(
            IInputReader reader,
            IFileLoader loader)
        {
            this.reader = reader;
            this.loader = loader;
        }

        public ContentTreeNode GetRootNode() => GetPathNode(null, ".");

        private bool IsLocalMaterialPath(string localPath)
        {
            var matFile = PathEx.Join(localPath, "pbr.yml");
            if (reader.FileExists(matFile)) return true;

            return loader.EnableAutoMaterial && loader.IsLocalMaterialPath(localPath);
        }

        public ContentTreeNode GetPathNode(ContentTreeNode parent, string localPath)
        {
            var isMat = IsLocalMaterialPath(localPath);

            var node = isMat
                ? new ContentTreeMaterialDirectory(parent) {
                    MaterialFilename = PathEx.Join(localPath, "pbr.yml"),
                }
                : new ContentTreeDirectory(parent);

            node.Name = Path.GetFileName(localPath);
            node.LocalPath = localPath;

            foreach (var childPath in reader.EnumerateDirectories(localPath, "*")) {
                var childNode = GetPathNode(node, childPath);
                node.Nodes.Add(childNode);
            }

            foreach (var file in reader.EnumerateFiles(localPath, "*.*")) {
                var fileName = Path.GetFileName(file);

                if (isMat && string.Equals(fileName, "pbr.yml", StringComparison.InvariantCultureIgnoreCase)) continue;

                var childNode = new ContentTreeFile(node) {
                    Name = fileName,
                    Filename = file,
                    Type = GetNodeType(fileName),
                    Icon = GetNodeIcon(fileName),
                };

                node.Nodes.Add(childNode);
            }

            return node;
        }

        private static ContentNodeType GetNodeType(string fileName)
        {
            if (string.Equals("input.yml", fileName, StringComparison.InvariantCultureIgnoreCase))
                return ContentNodeType.PackInput;

            if (fileName.EndsWith(".pack.yml", StringComparison.InvariantCultureIgnoreCase))
                return ContentNodeType.PackProfile;

            if (string.Equals("pbr.yml", fileName, StringComparison.InvariantCultureIgnoreCase)
                || fileName.EndsWith(".pbr.yml", StringComparison.InvariantCultureIgnoreCase))
                return ContentNodeType.Material;

            var ext = Path.GetExtension(fileName);
            if (ImageExtensions.Supports(ext)) return ContentNodeType.Texture;

            return ContentNodeType.Unknown;
        }

        private static PackIconKind GetNodeIcon(string fileName)
        {
            if (string.Equals("input.yml", fileName, StringComparison.InvariantCultureIgnoreCase))
                return PackIconKind.Palette;

            if (fileName.EndsWith(".pack.yml", StringComparison.InvariantCultureIgnoreCase))
                return PackIconKind.Export;

            if (string.Equals("pbr.yml", fileName, StringComparison.InvariantCultureIgnoreCase)
                || fileName.EndsWith(".pbr.yml", StringComparison.InvariantCultureIgnoreCase))
                return PackIconKind.FileChart;

            var ext = Path.GetExtension(fileName);
            if (ImageExtensions.Supports(ext)) return PackIconKind.Image;

            return PackIconKind.File;
        }
    }
}
