using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.Common.IO.Publishing
{
    public interface IPublisher
    {
        Task PublishAsync(ResourcePackContext context, bool clean, CancellationToken token = default);
    }

    internal abstract class PublisherBase<TMapping> : IPublisher
        where TMapping : IPublisherMapping
    {
        private readonly IServiceProvider provider;
        private readonly IInputReader reader;
        private readonly IPublishReader loader;

        protected ILogger<IPublisher> Logger {get;}
        protected IOutputWriter Writer {get;}
        protected TMapping Mapping {get;}


        protected PublisherBase(
            ILogger<IPublisher> logger,
            IServiceProvider provider,
            IPublishReader loader,
            IInputReader reader,
            IOutputWriter writer,
            TMapping mapping)
        {
            this.provider = provider;
            this.loader = loader;
            this.reader = reader;
            Mapping = mapping;
            Logger = logger;

            Writer = writer;
        }

        public async Task PublishAsync(ResourcePackContext context, bool clean, CancellationToken token = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            loader.EnableAutoMaterial = context.AutoMaterial;

            if (clean) {
                Logger.LogDebug("Cleaning destination...");

                try {
                    Writer.Clean();

                    Logger.LogInformation("Destination directory clean.");
                }
                catch (Exception error) {
                    Logger.LogError(error, "Failed to clean destination!");
                    throw new ApplicationException("Failed to clean destination!", error);
                }
            }

            await PublishPackMetaAsync(context.Profile, token);

            await PublishContentAsync(context, token);
        }

        private async Task PublishContentAsync(ResourcePackContext packContext, CancellationToken token = default)
        {
            var genericPublisher = new GenericTexturePublisher(packContext.Profile, reader, Writer);
            var packWriteTime = reader.GetWriteTime(packContext.Profile.LocalFile) ?? DateTime.Now;

            await foreach (var fileObj in loader.LoadAsync(token)) {
                token.ThrowIfCancellationRequested();

                switch (fileObj) {
                    case MaterialProperties material:
                        if (TryMapMaterial(material)) {
                            using var scope = provider.CreateScope();
                            var graphContext = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();
                            var graphBuilder = scope.ServiceProvider.GetRequiredService<ITextureGraphBuilder>();

                            graphContext.Input = packContext.Input;
                            graphContext.Profile = packContext.Profile;
                            graphContext.Material = material;
                            graphContext.Mapping = Mapping;

                            await graphBuilder.ProcessInputGraphAsync(token);
                            await OnMaterialPublishedAsync(scope.ServiceProvider, token);
                        }
                        else {
                            Logger.LogWarning("Skipping non-mapped material '{Name}'.", material.Name);
                        }
                        break;

                    case string localFile:
                        if (TryMapFile(localFile, out var destFile)) {
                            await PublishFileAsync(genericPublisher, packWriteTime, localFile, destFile, token);
                        }
                        else {
                            Logger.LogWarning("Skipping non-mapped file '{localFile}'.", localFile);
                        }
                        break;
                }
            }
        }

        protected abstract Task PublishPackMetaAsync(ResourcePackProfileProperties pack, CancellationToken token);

        protected virtual bool TryMapFile(in string sourceFile, out string destinationFile)
        {
            destinationFile = sourceFile;
            return true;
        }

        protected virtual bool TryMapMaterial(in MaterialProperties material)
        {
            return true;
            //if (Mapping == null) return true;

            //var sourcePath = material.LocalPath;
            //string sourceFile;

            //if (material.Parts?.Any(part => {
            //    sourceFile = PathEx.Join(sourcePath, part.Name);
            //    sourceFile = PathEx.Normalize(sourceFile);

            //    // TODO: replace with contains
            //    return Mapping.TryMap(sourceFile, out _);
            //}) ?? false) return true;

            //if (material.CTM?.Type != null) {
            //    // TODO
            //    return false;
            //}

            //sourceFile = PathEx.Join(sourcePath, material.Name);
            //sourceFile = PathEx.Normalize(sourceFile);

            //return Mapping.TryMap(sourceFile, out _);
        }

        protected virtual async Task PublishFileAsync(GenericTexturePublisher genericPublisher, DateTime packWriteTime, string sourceFile, string destFile, CancellationToken token)
        {
            var file = Path.GetFileName(sourceFile);
            if (fileIgnoreList.Contains(file)) {
                Logger.LogDebug("Skipping ignored file {sourceFile}.", sourceFile);
                return;
            }

            var sourceTime = reader.GetWriteTime(sourceFile);
            var destinationTime = Writer.GetWriteTime(destFile);

            if (IsUpToDate(packWriteTime, sourceTime, destinationTime)) {
                Logger.LogDebug("Skipping up-to-date untracked file {sourceFile}.", sourceFile);
                return;
            }

            if (IsGenericResizable(sourceFile)) {
                await genericPublisher.PublishAsync(sourceFile, null, destFile, token);
            }
            else {
                await using var srcStream = reader.Open(sourceFile);
                await using var destStream = Writer.Open(destFile);
                await srcStream.CopyToAsync(destStream, token);
            }

            Logger.LogInformation("Published untracked file {destFile}.", destFile);
        }

        protected virtual Task OnMaterialPublishedAsync(IServiceProvider scopeProvider, CancellationToken token) => Task.CompletedTask;

        //protected virtual Task OnPartPublishedAsync(ITextureGraphContext context, string partName, CancellationToken token)
        //{
        //    return Task.CompletedTask;
        //}

        protected static async Task WriteJsonAsync(Stream stream, object content, Formatting formatting, CancellationToken token)
        {
            await using var writer = new StreamWriter(stream);
            using var jsonWriter = new JsonTextWriter(writer) {Formatting = formatting};

            await JToken.FromObject(content).WriteToAsync(jsonWriter, token);
        }

        private static bool IsUpToDate(DateTime profileWriteTime, DateTime? sourceWriteTime, DateTime? destWriteTime)
        {
            if (!destWriteTime.HasValue || !sourceWriteTime.HasValue) return false;
            if (profileWriteTime > destWriteTime.Value) return false;
            return sourceWriteTime <= destWriteTime.Value;
        }

        private static bool IsGenericResizable(string localFile)
        {
            var extension = Path.GetExtension(localFile);
            if (!ImageExtensions.Supports(extension)) return false;

            // Do not resize pack icon
            var path = Path.GetDirectoryName(localFile);
            var name = Path.GetFileNameWithoutExtension(localFile);
            if (string.IsNullOrEmpty(path) && string.Equals("pack", name, StringComparison.InvariantCultureIgnoreCase)) return false;

            return !resizeIgnoreList.Any(x => localFile.StartsWith(x, StringComparison.InvariantCultureIgnoreCase));
        }

        private static readonly string[] resizeIgnoreList = {
            Path.Combine("assets", "minecraft", "textures", "font"),
            Path.Combine("assets", "minecraft", "textures", "gui"),
            Path.Combine("assets", "minecraft", "textures", "colormap"),
            Path.Combine("assets", "minecraft", "textures", "misc"),
            Path.Combine("assets", "minecraft", "optifine", "colormap"),
            Path.Combine("pack", "minecraft", "optifine", "colormap"),
        };

        private static readonly HashSet<string> fileIgnoreList = new(StringComparer.InvariantCultureIgnoreCase) {
            "input.yml",
            "source.txt",
            "readme.txt",
            "readme.md",
        };
    }
}
