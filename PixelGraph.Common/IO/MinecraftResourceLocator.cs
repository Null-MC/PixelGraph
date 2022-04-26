using PixelGraph.Common.Extensions;
using System.IO;

namespace PixelGraph.Common.IO
{
    public class MinecraftResourceLocator
    {
        private readonly IInputReader reader;


        public MinecraftResourceLocator(IInputReader reader)
        {
            this.reader = reader;
        }

        public bool FindLocalMaterial(string searchFile, out string localPath)
        {
            if (reader.FileExists(searchFile)) {
                localPath = searchFile;
                return true;
            }

            var texturesFile = PathEx.Join("assets/minecraft/textures", searchFile, "mat.yml");
            texturesFile = PathEx.Localize(texturesFile);

            if (reader.FileExists(texturesFile)) {
                localPath = texturesFile;
                return true;
            }

            var matName = Path.GetFileNameWithoutExtension(searchFile);

            var texturesBlockFile = PathEx.Join("assets/minecraft/textures/block", matName, "mat.yml");
            texturesBlockFile = PathEx.Localize(texturesBlockFile);

            if (reader.FileExists(texturesBlockFile)) {
                localPath = texturesBlockFile;
                return true;
            }

            // TODO: add textures/models

            var optifineCtmPath = PathEx.Join("assets/minecraft/optifine/ctm", matName);
            optifineCtmPath = PathEx.Localize(optifineCtmPath);

            if (TryFindFile(optifineCtmPath, "mat.yml", true, out localPath)) return true;

            localPath = null;
            return false;
        }

        public bool FindBlockModel(string searchFile, out string localPath)
        {
            if (!FindLocalBlockModel(searchFile, out localPath)) return false;
            localPath = reader.GetFullPath(localPath);
            return true;
        }

        public bool FindEntityModel(string searchFile, out string localPath)
        {
            if (!FindLocalEntityModel(searchFile, out localPath)) return false;
            localPath = reader.GetFullPath(localPath);
            return true;
        }

        private bool TryFindFile(string searchPath, string searchFile, bool recursive, out string localFile)
        {
            foreach (var file in reader.EnumerateFiles(searchPath, searchFile)) {
                localFile = file;
                return true;
            }

            if (recursive) {
                foreach (var path in reader.EnumerateDirectories(searchPath)) {
                    if (TryFindFile(path, searchFile, true, out localFile)) return true;
                }
            }

            localFile = null;
            return false;
        }

        private bool FindLocalBlockModel(string searchFile, out string localPath)
        {
            if (reader.FileExists(searchFile)) {
                localPath = searchFile;
                return true;
            }

            var modelsPath = PathEx.Join("assets/minecraft/models", searchFile);
            modelsPath = PathEx.Localize(modelsPath);

            if (reader.FileExists(modelsPath)) {
                localPath = modelsPath;
                return true;
            }

            var modelsBlockPath = PathEx.Join("assets/minecraft/models/block", searchFile);
            modelsBlockPath = PathEx.Localize(modelsBlockPath);

            if (reader.FileExists(modelsBlockPath)) {
                localPath = modelsBlockPath;
                return true;
            }

            localPath = null;
            return false;
        }

        private bool FindLocalEntityModel(string searchFile, out string localPath)
        {
            if (reader.FileExists(searchFile)) {
                localPath = searchFile;
                return true;
            }

            var modelsPath = PathEx.Join("assets/minecraft/optifine/jem", searchFile);
            modelsPath = PathEx.Localize(modelsPath);

            if (reader.FileExists(modelsPath)) {
                localPath = modelsPath;
                return true;
            }

            localPath = null;
            return false;
        }
    }
}
