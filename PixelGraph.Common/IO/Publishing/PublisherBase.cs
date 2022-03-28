using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures.Graphing;
using PixelGraph.Common.Textures.Graphing.Builders;
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
        int Concurrency {get; set;}

        Task PublishAsync(ResourcePackContext context, bool clean, CancellationToken token = default);
    }

    internal abstract class PublisherBase : IPublisher
    {
        public abstract int Concurrency {get; set;}


        public abstract Task PublishAsync(ResourcePackContext context, bool clean, CancellationToken token = default);

        protected static readonly string[] ResizeIgnoreList = {
            Path.Combine("assets", "minecraft", "textures", "font"),
            Path.Combine("assets", "minecraft", "textures", "gui"),
            Path.Combine("assets", "minecraft", "textures", "colormap"),
            Path.Combine("assets", "minecraft", "textures", "misc"),
            Path.Combine("assets", "minecraft", "optifine", "colormap"),
            Path.Combine("pack", "minecraft", "optifine", "colormap"),
        };

        protected static readonly HashSet<string> FileIgnoreList = new(StringComparer.InvariantCultureIgnoreCase) {
            "input.yml",
            "source.txt",
            "readme.txt",
            "readme.md",
        };
    }

    internal abstract class PublisherBase<TMapping> : PublisherBase
        where TMapping : IPublisherMapping
    {
        protected IServiceProvider Provider {get;}
        private readonly IPublishReader loader;

        protected ILogger<IPublisher> Logger {get;}
        protected IInputReader Reader {get;}
        protected IOutputWriter Writer {get;}
        protected TMapping Mapping {get;}

        public override int Concurrency {get; set;} = 1;


        protected PublisherBase(
            ILogger<IPublisher> logger,
            IServiceProvider provider,
            IPublishReader loader,
            IInputReader reader,
            IOutputWriter writer,
            TMapping mapping)
        {
            Provider = provider;
            this.loader = loader;

            Reader = reader;
            Mapping = mapping;
            Logger = logger;

            Writer = writer;
        }

        public override async Task PublishAsync(ResourcePackContext context, bool clean, CancellationToken token = default)
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

            await OnPackPublished(context, token);
        }

        protected virtual Task OnPackPublished(ResourcePackContext context, CancellationToken token) => Task.CompletedTask;

        private async Task PublishContentAsync(ResourcePackContext packContext, CancellationToken token = default)
        {
            var genericPublisher = new GenericTexturePublisher(packContext.Profile, Reader, Writer);
            var packWriteTime = Reader.GetWriteTime(packContext.Profile.LocalFile) ?? DateTime.Now;

            await loader.LoadAsync(token).AsyncParallelForEach(async fileObj => {
                switch (fileObj) {
                    case MaterialProperties material:
                        if (material.CTM?.Method != null) {
                            var publishConnected = packContext.Profile.PublishConnected ?? ResourcePackProfileProperties.PublishConnectedDefault;

                            if (!publishConnected) {
                                Logger.LogDebug("Skipping connected texture '{DisplayName}'. feature disabled.", material.DisplayName);
                                break;
                            }
                        }

                        if (TryMapMaterial(material)) {
                            using var scope = Provider.CreateScope();
                            var graphContext = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();
                            var graphBuilder = scope.ServiceProvider.GetRequiredService<IPublishGraphBuilder>();

                            graphContext.Input = packContext.Input;
                            graphContext.Profile = packContext.Profile;
                            graphContext.Material = material;
                            graphContext.Mapping = Mapping;

                            await graphBuilder.PublishAsync(token);
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
            }, Concurrency, null, token);
        }

        protected abstract Task PublishPackMetaAsync(ResourcePackProfileProperties pack, CancellationToken token);

        protected virtual bool TryMapFile(in string sourceFile, out string destinationFile)
        {
            destinationFile = sourceFile;
            return true;
        }

        protected virtual bool TryMapMaterial(in MaterialProperties material) => true;

        protected virtual async Task PublishFileAsync(GenericTexturePublisher genericPublisher, DateTime packWriteTime, string sourceFile, string destFile, CancellationToken token)
        {
            var file = Path.GetFileName(sourceFile);
            if (FileIgnoreList.Contains(file)) {
                Logger.LogDebug("Skipping ignored file {sourceFile}.", sourceFile);
                return;
            }

            var sourceTime = Reader.GetWriteTime(sourceFile);
            var destinationTime = Writer.GetWriteTime(destFile);

            if (IsUpToDate(packWriteTime, sourceTime, destinationTime)) {
                Logger.LogDebug("Skipping up-to-date untracked file {sourceFile}.", sourceFile);
                return;
            }

            if (IsGenericResizable(sourceFile)) {
                await genericPublisher.PublishAsync(sourceFile, null, destFile, token);
            }
            else {
                await using var srcStream = Reader.Open(sourceFile);
                await Writer.OpenWriteAsync(destFile, async destStream => {
                    await srcStream.CopyToAsync(destStream, token);
                }, token);
            }

            Logger.LogInformation("Published untracked file {destFile}.", destFile);
        }

        protected virtual Task OnMaterialPublishedAsync(IServiceProvider scopeProvider, CancellationToken token) => Task.CompletedTask;

        protected static async Task WriteJsonAsync(Stream stream, object content, Formatting formatting, CancellationToken token)
        {
            await using var writer = new StreamWriter(stream);
            using var jsonWriter = new JsonTextWriter(writer) {
                Formatting = formatting,
            };

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

            return !ResizeIgnoreList.Any(x => localFile.StartsWith(x, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
