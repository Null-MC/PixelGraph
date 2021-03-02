using MaterialDesignThemes.Wpf;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PixelGraph.UI.Internal
{
    internal interface IContentTreeReader
    {
        void Update(ContentTreeNode parentNode);

        //ContentTreeNode GetRootNode();
        //ContentTreeNode GetPathNode(ContentTreeNode parent, string localPath);
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

        public void Update(ContentTreeNode parentNode)
        {
            var existingNodes = parentNode.Nodes.ToList();

            foreach (var childPath in reader.EnumerateDirectories(parentNode.LocalPath, "*")) {
                var isMat = IsLocalMaterialPath(childPath);

                var existingNode = TryRemove(existingNodes, x => {
                    if (isMat && !(x is ContentTreeMaterialDirectory)) return false;
                    if (!isMat && !(x is ContentTreeDirectory)) return false;
                    return string.Equals(x.LocalPath, childPath, StringComparison.InvariantCultureIgnoreCase);
                });

                // Add new folder nodes
                if (existingNode == null) {
                    existingNode = isMat
                        ? new ContentTreeMaterialDirectory(parentNode) {
                            MaterialFilename = PathEx.Join(childPath, "pbr.yml"),
                        }
                        : new ContentTreeDirectory(parentNode);

                    existingNode.Name = Path.GetFileName(childPath);
                    existingNode.LocalPath = childPath;
                    parentNode.Nodes.Add(existingNode);
                }

                Update(existingNode);
            }

            var isParentMat = parentNode is ContentTreeMaterialDirectory;
            foreach (var file in reader.EnumerateFiles(parentNode.LocalPath, "*.*")) {
                var fileName = Path.GetFileName(file);
                if (isParentMat && string.Equals(fileName, "pbr.yml", StringComparison.InvariantCultureIgnoreCase)) continue;

                var existingNode = TryRemove(existingNodes, x => {
                    if (!(x is ContentTreeFile fileNode)) return false;
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

        private static ContentTreeNode TryRemove(IList<ContentTreeNode> nodes, Func<ContentTreeNode, bool> filter)
        {
            for (var i = nodes.Count - 1; i >= 0; i--) {
                var node = nodes[i];
                if (!filter(node)) continue;
                    
                nodes.RemoveAt(i);
                return node;
            }

            return null;
        }


        //public ContentTreeNode GetRootNode() => GetPathNode(null, ".");

        private bool IsLocalMaterialPath(string localPath)
        {
            var matFile = PathEx.Join(localPath, "pbr.yml");
            if (reader.FileExists(matFile)) return true;

            return loader.EnableAutoMaterial && loader.IsLocalMaterialPath(localPath);
        }

        //public ContentTreeNode GetPathNode(ContentTreeNode parent, string localPath)
        //{
        //    var isMat = IsLocalMaterialPath(localPath);

        //    var node = isMat
        //        ? new ContentTreeMaterialDirectory(parent) {
        //            MaterialFilename = PathEx.Join(localPath, "pbr.yml"),
        //        }
        //        : new ContentTreeDirectory(parent);

        //    node.Name = Path.GetFileName(localPath);
        //    node.LocalPath = localPath;

        //    foreach (var childPath in reader.EnumerateDirectories(localPath, "*")) {
        //        var childNode = GetPathNode(node, childPath);
        //        node.Nodes.Add(childNode);
        //    }

        //    foreach (var file in reader.EnumerateFiles(localPath, "*.*")) {
        //        var fileName = Path.GetFileName(file);

        //        if (isMat && string.Equals(fileName, "pbr.yml", StringComparison.InvariantCultureIgnoreCase)) continue;

        //        var childNode = new ContentTreeFile(node) {
        //            Name = fileName,
        //            Filename = file,
        //            Type = GetNodeType(fileName),
        //            Icon = GetNodeIcon(fileName),
        //        };

        //        node.Nodes.Add(childNode);
        //    }

        //    return node;
        //}

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
