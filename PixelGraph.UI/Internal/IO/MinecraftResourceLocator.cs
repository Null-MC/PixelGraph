using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.UI.Internal.IO.Resources;
using PixelGraph.UI.Internal.Projects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PixelGraph.UI.Internal.IO
{
    public class MinecraftResourceLocator
    {
        private readonly IServiceProvider provider;
        private readonly IProjectContextManager projectContextMgr;
        private readonly IResourceLocationManager resourceMgr;


        public MinecraftResourceLocator(IServiceProvider provider)
        {
            this.provider = provider;

            projectContextMgr = provider.GetRequiredService<IProjectContextManager>();
            resourceMgr = provider.GetRequiredService<IResourceLocationManager>();
        }

        public bool FindLocalMaterial(string searchFile, out string localPath)
        {
            var builder = GetProjectBuilder();
            using var scope = builder.Build();
            var reader = scope.GetRequiredService<IInputReader>();

            if (reader.FileExists(searchFile)) {
                localPath = searchFile;
                return true;
            }

            foreach (var resourcePath in FindAllNamespaceDirectories(reader)) {
                var texturesFile = PathEx.Join(resourcePath, "textures", searchFile, "mat.yml");
                texturesFile = PathEx.Localize(texturesFile);

                if (reader.FileExists(texturesFile)) {
                    localPath = texturesFile;
                    return true;
                }

                var matName = Path.GetFileNameWithoutExtension(searchFile);

                var texturesBlockFile = PathEx.Join(resourcePath, "textures/block", matName, "mat.yml");
                texturesBlockFile = PathEx.Localize(texturesBlockFile);

                if (reader.FileExists(texturesBlockFile)) {
                    localPath = texturesBlockFile;
                    return true;
                }

                // TODO: add textures/models

                var optifineCtmPath = PathEx.Join(resourcePath, "optifine/ctm", matName);
                optifineCtmPath = PathEx.Localize(optifineCtmPath);

                if (TryFindFile(reader, optifineCtmPath, "mat.yml", true, out localPath)) return true;
            }

            localPath = null;
            return false;
        }

        public bool FindBlockModel(string searchFile, Action<Stream> readAction)
        {
            foreach (var builder in EnumerateScopes()) {
                using var scope = builder.Build();
                var reader = scope.GetRequiredService<IInputReader>();

                if (!FindLocalBlockModel(reader, searchFile, out var localPath)) continue;

                //localPath = reader.GetFullPath(localPath);
                using var stream = reader.Open(localPath);
                readAction(stream);
                return true;
            }

            //localPath = null;
            return false;
        }

        public bool FindEntityModel(string searchFile, out string localPath)
        {
            foreach (var builder in EnumerateScopes()) {
                using var scope = builder.Build();
                var reader = scope.GetRequiredService<IInputReader>();

                if (!FindLocalEntityModel(reader, searchFile, out localPath)) continue;

                localPath = reader.GetFullPath(localPath);
                return true;
            }

            localPath = null;
            return false;
        }

        private static bool TryFindFile(IInputReader reader, string searchPath, string searchFile, bool recursive, out string localFile)
        {
            foreach (var file in reader.EnumerateFiles(searchPath, searchFile)) {
                localFile = file;
                return true;
            }

            if (recursive) {
                foreach (var path in reader.EnumerateDirectories(searchPath)) {
                    if (TryFindFile(reader, path, searchFile, true, out localFile)) return true;
                }
            }

            localFile = null;
            return false;
        }

        private bool FindLocalBlockModel(IInputReader reader, string searchFile, out string localPath)
        {
            if (reader.FileExists(searchFile)) {
                localPath = searchFile;
                return true;
            }

            TryExtractNamespace(ref searchFile, out var @namespace);
            @namespace ??= "minecraft";

            foreach (var resourcePath in FindAllNamespaceDirectories(reader, @namespace)) {
                var modelsPath = PathEx.Join(resourcePath, "models", searchFile);
                modelsPath = PathEx.Localize(modelsPath);

                if (reader.FileExists(modelsPath)) {
                    localPath = modelsPath;
                    return true;
                }

                var modelsBlockPath = PathEx.Join(resourcePath, "models/block", searchFile);
                modelsBlockPath = PathEx.Localize(modelsBlockPath);

                if (reader.FileExists(modelsBlockPath)) {
                    localPath = modelsBlockPath;
                    return true;
                }
            }

            localPath = null;
            return false;
        }

        private bool FindLocalEntityModel(IInputReader reader, string searchFile, out string localPath)
        {
            if (reader.FileExists(searchFile)) {
                localPath = searchFile;
                return true;
            }

            TryExtractNamespace(ref searchFile, out var @namespace);
            @namespace ??= "minecraft";

            foreach (var resourcePath in FindAllNamespaceDirectories(reader, @namespace)) {
                var modelsPath = PathEx.Join(resourcePath, "optifine/jem", searchFile);
                modelsPath = PathEx.Localize(modelsPath);

                if (reader.FileExists(modelsPath)) {
                    localPath = modelsPath;
                    return true;
                }
            }

            localPath = null;
            return false;
        }

        public IEnumerable<string> FindAllNamespaces()
        {
            var builder = GetProjectBuilder();
            using var scope = builder.Build();

            var reader = scope.GetRequiredService<IInputReader>();
            return FindAllNamespaceDirectories(reader).Select(Path.GetFileName);
        }

        private IEnumerable<string> FindAllNamespaceDirectories(IInputReader reader, string @namespace = null)
        {
            if (@namespace != null) return new[] { $"assets/{@namespace}" };
            return reader.EnumerateDirectories("assets");
        }

        private IEnumerable<IServiceBuilder> EnumerateScopes()
        {
            yield return GetProjectBuilder();
            var locations = resourceMgr.GetLocations();
            if (locations == null) yield break;

            foreach (var resourceFile in locations) {
                var resourceBuilder = provider.GetRequiredService<IServiceBuilder>();
                resourceBuilder.ConfigureReader(ContentTypes.Archive, GameEditions.Java, resourceFile.File);
                yield return resourceBuilder;
            }
        }

        private IServiceBuilder GetProjectBuilder()
        {
            var projectContext = projectContextMgr.GetContext();

            var projectBuilder = provider.GetRequiredService<IServiceBuilder>();
            projectBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);
            return projectBuilder;
        }

        private static bool TryExtractNamespace(ref string path, out string @namespace)
        {
            var nsIndex = path.IndexOf(':');

            if (nsIndex >= 0) {
                @namespace = path[..nsIndex];
                path = path[(nsIndex + 1)..];
                return true;
            }

            @namespace = null;
            return false;
        }
    }
}
