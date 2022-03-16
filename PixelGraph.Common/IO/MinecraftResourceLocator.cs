using PixelGraph.Common.Extensions;

namespace PixelGraph.Common.IO
{
    public interface IMinecraftResourceLocator
    {
        bool FindLocalMaterial(string searchFile, out string localPath);
        //bool FindLocalTexture(string searchFile, out string localPath);

        bool FindBlockModel(string searchFile, out string localPath);
        bool FindEntityModel(string searchFile, out string localPath);
        //bool FindLocalBlockModel(string searchFile, out string localPath);
        //bool FindLocalEntityModel(string searchFile, out string localPath);
    }

    public class MinecraftResourceLocator : IMinecraftResourceLocator
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

            var texturesPath = PathEx.Join("assets/minecraft/textures", searchFile, "mat.yml");
            texturesPath = PathEx.Localize(texturesPath);

            if (reader.FileExists(texturesPath)) {
                localPath = texturesPath;
                return true;
            }

            var modelsBlockPath = PathEx.Join("assets/minecraft/textures/block", searchFile, "mat.yml");
            modelsBlockPath = PathEx.Localize(modelsBlockPath);

            if (reader.FileExists(modelsBlockPath)) {
                localPath = modelsBlockPath;
                return true;
            }

            localPath = null;
            return false;
        }

        //public bool FindLocalTexture(string searchFile, out string localPath)
        //{
        //    //
        //}

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
