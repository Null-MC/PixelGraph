using Microsoft.Extensions.Logging;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.Material;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.Internal.Utilities
{
    public interface ITextureEditUtility
    {
        Task<bool> EditLayerAsync(MaterialProperties material, string textureTag, CancellationToken token = default);
    }

    internal class TextureEditUtility : ITextureEditUtility
    {
        private readonly ILogger<TextureEditUtility> logger;
        private readonly IAppSettings appSettings;
        private readonly IInputReader reader;
        //private readonly INamingStructure naming;


        public TextureEditUtility(
            ILogger<TextureEditUtility> logger,
            IAppSettings appSettings,
            IInputReader reader)
            //INamingStructure naming)
        {
            this.appSettings = appSettings;
            this.reader = reader;
            //this.naming = naming;
            this.logger = logger;
        }

        public async Task<bool> EditLayerAsync(MaterialProperties material, string textureTag, CancellationToken token = default)
        {
            var inputFile = TextureTags.Get(material, textureTag);

            if (string.IsNullOrWhiteSpace(inputFile))
                inputFile = reader.EnumerateInputTextures(material, textureTag).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(inputFile)) {
                // TODO: determine filename from naming convention
                var matchName = NamingStructure.GetInputTextureName(material, textureTag);
                var srcPath = material.UseGlobalMatching
                    ? material.LocalPath : PathEx.Join(material.LocalPath, material.Name);

                inputFile = PathEx.Join(srcPath, matchName.Replace("*", "png"));
            }

            if (string.IsNullOrWhiteSpace(inputFile))
                throw new ApplicationException("Unable to determine texture filename!");

            var inputFileFull = reader.GetFullPath(inputFile);
            var tempFile = $"{Path.GetTempFileName()}.png";

            try {
                if (File.Exists(inputFileFull)) {
                    File.Copy(inputFileFull, tempFile);
                }
                else {
                    // create new file
                    // TODO: detect hasColor and hasAlpha from input encoding
                    using var newImage = CreateImage(1, 1, true, true);
                    await newImage.SaveAsPngAsync(tempFile, token);
                }

                var commandSplitIndex = appSettings.Data.TextureEditCommand.IndexOf(' ');

                var info = new ProcessStartInfo {
                    UseShellExecute = false,
                    FileName = appSettings.Data.TextureEditCommand[..commandSplitIndex],
                    Arguments = appSettings.Data.TextureEditCommand[(commandSplitIndex+1)..],
                };

                var srcTime = File.GetLastWriteTimeUtc(tempFile);
                using var process = Process.Start(info);

                if (process == null)
                    throw new ApplicationException($"Failed to start process '{info.FileName}'!");

                // TODO: make async
                process.WaitForExit();

                if (!File.Exists(tempFile))
                    throw new ApplicationException("Unable to locate edited temp file!");

                var editTime = File.GetLastWriteTimeUtc(tempFile);

                if (editTime <= srcTime) return false;

                File.Copy(tempFile, inputFileFull, true);
                return true;
            }
            finally {
                if (File.Exists(tempFile)) {
                    try {
                        File.Delete(tempFile);
                    }
                    catch (Exception error) {
                        logger.LogWarning(error, "Failed to delete temporary image file!");
                    }
                }
            }
        }

        private Image CreateImage(int width, int height, bool hasColor, bool hasAlpha)
        {
            if (hasColor) {
                if (hasAlpha)
                    return new Image<Rgba32>(Configuration.Default, width, height);

                return new Image<Rgb24>(Configuration.Default, width, height);
            }

            if (hasAlpha) throw new ApplicationException("Transparent greyscale textures not supported!");

            return new Image<L8>(Configuration.Default, width, height);
        }
    }
}
