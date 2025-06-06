﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures.Graphing;
using PixelGraph.Common.Textures.Graphing.Builders;

namespace PixelGraph.Common.IO.Publishing;

public interface IPublisher
{
    event EventHandler<PublishStatus> StateChanged;

    int Concurrency {get; set;}

    Task PrepareAsync(ProjectPublishContext context, bool clean, CancellationToken token = default);
    Task PublishAsync(ProjectPublishContext context, CancellationToken token = default);
}

public readonly struct PublishStatus
{
    public bool IsAnalyzing {get; init;}
    public double Progress {get; init;}
}

public abstract class PublisherBase(
    ILogger<IPublisher> logger,
    IServiceProvider provider) : IPublisher
{
    private readonly IPublishReader loader = provider.GetRequiredService<IPublishReader>();
    private readonly IPublishSummary summary = provider.GetRequiredService<IPublishSummary>();
    private int totalMaterialCount, totalFileCount;
    private int currentMaterialCount, currentFileCount;
    private object[]? content;

    public event EventHandler<PublishStatus>? StateChanged;

    protected IServiceProvider Provider {get;} = provider;
    protected ILogger<IPublisher> Logger {get;} = logger;
    protected IPublisherMapping Mapping {get; init;} = new DefaultPublishMapping();
    protected IInputReader Reader {get;} = provider.GetRequiredService<IInputReader>();
    protected IOutputWriter Writer {get;} = provider.GetRequiredService<IOutputWriter>();

    public int Concurrency {get; set;} = 1;


    public virtual async Task PrepareAsync(ProjectPublishContext context, bool clean, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.Project);
        ArgumentNullException.ThrowIfNull(context.Profile);

        loader.EnableAutoMaterial = context.Project.Input?.AutoMaterial ?? PackInputEncoding.AutoMaterialDefault;

        summary.Reset();
        if (clean) CleanDestination();

        await PublishPackMetaAsync(context, token);

        content = await loader.LoadAsync(token).ToArrayAsync(token);

        totalMaterialCount = content.Count(o => o is MaterialProperties);
        totalFileCount = content.Length - totalMaterialCount;
    }

    public virtual async Task PublishAsync(ProjectPublishContext context, CancellationToken token = default)
    {
        await PublishContentAsync(context, token);

        await OnPackPublished(context, token);
    }

    protected virtual void CleanDestination()
    {
        Logger.LogDebug("Cleaning destination...");

        try {
            Writer.Clean();
            Logger.LogInformation("Destination directory cleaned.");
        }
        catch (Exception error) {
            Logger.LogError(error, "Failed to clean destination!");
            throw new ApplicationException("Failed to clean destination!", error);
        }
    }

    protected virtual Task OnPackPublished(ProjectPublishContext packContext, CancellationToken token) => Task.CompletedTask;

    private async Task PublishContentAsync(ProjectPublishContext packContext, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        currentMaterialCount = 0;
        currentFileCount = 0;

        await content.AsyncParallelForEach(async fileObj => {
            switch (fileObj) {
                case MaterialProperties material:
                    await PublishMaterialAsync(packContext, material, token);
                    Interlocked.Increment(ref currentMaterialCount);
                    break;
                case string localFile:
                    await PublishFileAsync(packContext, localFile, token);
                    Interlocked.Increment(ref currentFileCount);
                    break;
            }

            var materialProgress = currentMaterialCount / (double)totalMaterialCount;
            var fileProgress = currentFileCount / (double)totalFileCount;

            var status = new PublishStatus {
                Progress = 0.9d * materialProgress + 0.1d * fileProgress,
            };

            OnStateChanged(ref status);
        }, Concurrency, null, token);
    }

    private async Task PublishMaterialAsync(ProjectPublishContext packContext, MaterialProperties material, CancellationToken token)
    {
        if (material.CTM?.Method != null) {
            var publishConnected = packContext.Profile?.PublishConnected ?? PublishProfileProperties.PublishConnectedDefault;

            if (!publishConnected) {
                Logger.LogDebug("Skipping connected texture '{DisplayName}'. feature disabled.", material.DisplayName);
                return;
            }
        }

        if (TryMapMaterial(material)) {
            using var scope = Provider.CreateScope();
            var graphContext = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();
            var graphBuilder = scope.ServiceProvider.GetRequiredService<IPublishGraphBuilder>();

            graphContext.PackWriteTime = packContext.LastUpdated;
            graphContext.Project = packContext.Project;
            graphContext.Profile = packContext.Profile;
            graphContext.Material = material;
            graphContext.Mapping = Mapping;

            await graphBuilder.PublishAsync(token);
            await OnMaterialPublishedAsync(scope.ServiceProvider, token);
        }
        else {
            Logger.LogWarning("Skipping non-mapped material '{Name}'.", material.Name);
        }
    }

    private async Task PublishFileAsync(ProjectPublishContext packContext, string localFile, CancellationToken token)
    {
        if (!TryMapFile(localFile, out var destFile) || destFile == null) {
            Logger.LogWarning("Skipping non-mapped file '{localFile}'.", localFile);
            return;
        }

        using var scope = Provider.CreateScope();
        var graphContext = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();
        var genericPublisher = scope.ServiceProvider.GetRequiredService<GenericTexturePublisher>();

        graphContext.PackWriteTime = packContext.LastUpdated;
        graphContext.Project = packContext.Project;
        graphContext.Profile = packContext.Profile;
        graphContext.Mapping = Mapping;

        if (!PublishIgnore.AllowFile(localFile)) {
            Logger.LogDebug("Skipping ignored file {sourceFile}.", localFile);
            return;
        }

        var sourceTime = Reader.GetWriteTime(localFile);
        var destinationTime = Writer.GetWriteTime(destFile);

        if (IsUpToDate(packContext.LastUpdated, sourceTime, destinationTime)) {
            Logger.LogDebug("Skipping up-to-date untracked file {sourceFile}.", localFile);
            return;
        }

        if (IsGenericResizable(localFile)) { await genericPublisher.PublishAsync(localFile, null, destFile, token); }
        else {
            await using var srcStream = Reader.Open(localFile)
                ?? throw new ApplicationException("Failed to open source file stream!");

            await Writer.OpenWriteAsync(destFile, async destStream => {await srcStream.CopyToAsync(destStream, token);}, token);
        }

        Logger.LogInformation("Published untracked file {destFile}.", destFile);
    }

    protected abstract Task PublishPackMetaAsync(ProjectPublishContext context, CancellationToken token);

    protected virtual bool TryMapFile(in string sourceFile, out string? destinationFile)
    {
        destinationFile = sourceFile;
        return true;
    }

    protected virtual bool TryMapMaterial(in MaterialProperties material) => true;

    protected virtual Task OnMaterialPublishedAsync(IServiceProvider scopeProvider, CancellationToken token) => Task.CompletedTask;

    private void OnStateChanged(ref PublishStatus status)
    {
        StateChanged?.Invoke(this, status);
    }

    protected static async Task WriteJsonAsync(Stream stream, object content, Formatting formatting, CancellationToken token)
    {
        await using var writer = new StreamWriter(stream, leaveOpen: true);
        await using var jsonWriter = new JsonTextWriter(writer);
        jsonWriter.Formatting = formatting;

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

    private static readonly string[] resizeIgnoreList = [
        Path.Combine("assets", "minecraft", "textures", "font"),
        Path.Combine("assets", "minecraft", "textures", "gui"),
        Path.Combine("assets", "minecraft", "textures", "colormap"),
        Path.Combine("assets", "minecraft", "textures", "misc"),
        Path.Combine("assets", "minecraft", "optifine", "colormap"),
        Path.Combine("pack", "minecraft", "optifine", "colormap")
    ];
}