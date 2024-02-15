using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Common.Textures.Graphing;
using PixelGraph.Common.Textures.Graphing.Builders;

namespace PixelGraph.Common.IO.Importing;

internal interface IMaterialImporter
{
    /// <summary>
    /// Gets or sets whether imported materials should be global or local.
    /// </summary>
    bool AsGlobal {get; set;}

    IProjectDescription? Project {get; set;}

    PublishProfileProperties? PackProfile {get; set;}

    bool IsMaterialFile(string filename, out string name);

    Task<MaterialProperties> CreateMaterialAsync(string localPath, string name);
    Task ImportAsync(MaterialProperties material, CancellationToken token = default);
}

internal abstract class MaterialImporterBase(IServiceProvider provider) : IMaterialImporter
{
    private readonly IMaterialWriter matWriter = provider.GetRequiredService<IMaterialWriter>();

    protected IInputReader Reader {get;} = provider.GetRequiredService<IInputReader>();

    /// <inheritdoc />
    public bool AsGlobal {get; set;}

    public IProjectDescription? Project {get; set;}

    public PublishProfileProperties? PackProfile {get; set;}


    public abstract bool IsMaterialFile(string filename, out string name);

    public async Task<MaterialProperties> CreateMaterialAsync(string localPath, string name)
    {
        var matFile = AsGlobal
            ? PathEx.Join(localPath, $"{name}.mat.yml")
            : PathEx.Join(localPath, name, "mat.yml");

        var material = new MaterialProperties {
            Name = name,
            LocalPath = localPath,
            LocalFilename = matFile,
            UseGlobalMatching = AsGlobal,
        };

        await matWriter.WriteAsync(material);
        return material;
    }

    public async Task ImportAsync(MaterialProperties material, CancellationToken token = default)
    {
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();

        context.PackWriteTime = DateTime.Now;
        context.Project = (IProjectDescription?)Project?.Clone();
        context.Profile = (PublishProfileProperties?)PackProfile?.Clone();
        context.PublishAsGlobal = AsGlobal;
        context.Material = material;
        context.IsImport = true;

        context.Mapping = new DefaultPublishMapping();

        context.ApplyOutputEncoding();

        await OnImportMaterialAsync(scope.ServiceProvider, token);

        if (!context.Mapping.TryMap(material.LocalPath, material.Name, out var destPath, out var destName)) return;

        var fileName = AsGlobal ? $"{destName}.mat.yml" : "mat.yml";
        material.LocalPath = AsGlobal ? destPath : PathEx.Join(destPath, destName);
        material.LocalFilename = PathEx.Join(material.LocalPath, fileName);

        await matWriter.WriteAsync(material, token);
    }

    protected virtual Task OnImportMaterialAsync(IServiceProvider scope, CancellationToken token = default)
    {
        var graphBuilder = scope.GetRequiredService<IImportGraphBuilder>();

        return graphBuilder.ImportAsync(token);
    }
}