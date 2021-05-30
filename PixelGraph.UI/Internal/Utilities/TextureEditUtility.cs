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
        void Cancel();
    }

    internal class TextureEditUtility : ITextureEditUtility
    {
        private readonly ILogger<TextureEditUtility> logger;
        private readonly IAppSettings appSettings;
        private readonly IInputReader reader;

        private CancellationTokenSource mergedTokenSource;


        public TextureEditUtility(
            ILogger<TextureEditUtility> logger,
            IAppSettings appSettings,
            IInputReader reader)
        {
            this.appSettings = appSettings;
            this.reader = reader;
            this.logger = logger;
        }

        public Task<bool> EditLayerAsync(MaterialProperties material, string textureTag, CancellationToken token = default)
        {
            mergedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);

            try {
                return EditLayerInternalAsync(material, textureTag, mergedTokenSource.Token);
            }
            finally {
                mergedTokenSource.Dispose();
                mergedTokenSource = null;
            }
        }

        public async Task<bool> EditLayerInternalAsync(MaterialProperties material, string textureTag, CancellationToken token = default)
        {
            var inputFileFull = GetInputFilename(material, textureTag);

            var ext = Path.GetExtension(inputFileFull);
            var tempFile = $"{Path.GetTempFileName()}{ext}";

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

                var (exe, args) = ParseCommand(appSettings.Data.TextureEditCommand);

                args = args.Replace("$1", tempFile);

                var info = new ProcessStartInfo {
                    UseShellExecute = false,
                    FileName = exe,
                    Arguments = args,
                };

                var srcTime = File.GetLastWriteTimeUtc(tempFile);
                using var process = Process.Start(info);

                if (process == null)
                    throw new ApplicationException($"Failed to start process '{info.FileName}'!");

                try {
                    await process.WaitForExitAsync(token);
                }
                catch (OperationCanceledException) {}

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

        public void Cancel()
        {
            mergedTokenSource?.Cancel();
        }

        private string GetInputFilename(MaterialProperties material, string textureTag)
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

            return reader.GetFullPath(inputFile);
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

        private static (string exe, string args) ParseCommand(string command)
        {
            if (command.StartsWith('"')) {
                var commandSplitIndex = command.IndexOf('"', 1);
                if (commandSplitIndex < 0) return (null, null);
                
                var exe = command[1..commandSplitIndex];
                var args = command[(commandSplitIndex + 1)..].TrimStart();
                return (exe, args);
            }
            else {
                var commandSplitIndex = command.IndexOf(' ');
                if (commandSplitIndex < 0) return (null, null);

                var exe = command[..commandSplitIndex];
                var args = command[(commandSplitIndex + 1)..];
                return (exe, args);
            }
        }
    }
}
