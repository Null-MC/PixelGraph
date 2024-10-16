using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Common.Material;
using PixelGraph.Common.Projects;
using PixelGraph.Common.Textures.Graphing;
using PixelGraph.Common.Textures.Graphing.Builders;
using PixelGraph.Tests.Internal.Mocks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit.Abstractions;

namespace PixelGraph.Tests.Internal;

public abstract class ImageTestBase : TestBase
{
    protected ImageTestBase(ITestOutputHelper output) : base(output) {}

    protected ImageTestGraph Graph()
    {
        var provider = Builder.Build();
        return new ImageTestGraph(provider);
    }


    protected ImageTestGraph DefaultGraph(IProjectDescription project, PublishProfileProperties profile)
    {
        var graph = Graph();

        graph.Project = project;
        graph.PackProfile = profile;
        graph.Material = new MaterialProperties {
            Name = "test",
            LocalPath = "assets",
            LocalFilename = "mat.yml",
        };

        return graph;
    }
}

public class ImageTestGraph : IDisposable, IAsyncDisposable
{
    private readonly ServiceProvider provider;

    public IServiceProvider Provider => provider;
    public IProjectDescription? Project {get; set;}
    public PublishProfileProperties? PackProfile {get; set;}
    public MaterialProperties? Material {get; set;}


    public ImageTestGraph(ServiceProvider provider)
    {
        this.provider = provider;
    }

    public void Dispose()
    {
        provider.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await provider.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    public Task CreateImageAsync(string localFile, byte gray)
    {
        var content = provider.GetRequiredService<MockFileContent>();

        var color = new L8(gray);
        using var image = new Image<L8>(Configuration.Default, 1, 1, color);
        return content.AddAsync(localFile, image);
    }

    public Task CreateImageAsync(string localFile, byte r, byte g, byte b)
    {
        var content = provider.GetRequiredService<MockFileContent>();

        var color = new Rgb24(r, g, b);
        using var image = new Image<Rgb24>(Configuration.Default, 1, 1, color);
        return content.AddAsync(localFile, image);
    }

    public Task CreateImageAsync(string localFile, byte r, byte g, byte b, byte alpha)
    {
        var content = provider.GetRequiredService<MockFileContent>();

        var color = new Rgba32(r, g, b, alpha);
        using var image = new Image<Rgba32>(Configuration.Default, 1, 1, color);
        return content.AddAsync(localFile, image);
    }

    public Task CreateImageAsync<TPixel>(string localFile, int width, int height, Action<Image<TPixel>> initAction)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        var content = provider.GetRequiredService<MockFileContent>();

        using var image = new Image<TPixel>(Configuration.Default, width, height);

        initAction(image);

        return content.AddAsync(localFile, image);
    }

    public Task CreateFileAsync(string localFile, string text)
    {
        var content = provider.GetRequiredService<MockFileContent>();
        return content.AddAsync(localFile, text);
    }

    public async Task ProcessAsync(CancellationToken token = default)
    {
        using var scope = provider.CreateScope();
        var graphContext = scope.ServiceProvider.GetRequiredService<ITextureGraphContext>();
        var graphBuilder = scope.ServiceProvider.GetRequiredService<IPublishGraphBuilder>();

        graphContext.Project = Project;
        graphContext.Profile = PackProfile;
        graphContext.Material = Material;
        graphContext.Mapping = new DefaultPublishMapping();
        graphContext.PackWriteTime = DateTime.UtcNow;

        await graphBuilder.PublishAsync(token);
        await graphBuilder.PublishInventoryAsync(token);
    }

    public Task<Image<Rgba32>> GetImageAsync(string localFile)
    {
        var content = provider.GetRequiredService<MockFileContent>();
        return content.OpenImageAsync<Rgba32>(localFile);
    }

    public Task<Image<TPixel>> GetImageAsync<TPixel>(string localFile)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        var content = provider.GetRequiredService<MockFileContent>();
        return content.OpenImageAsync<TPixel>(localFile);
    }

    public Stream? GetFile(string localFile)
    {
        var content = provider.GetRequiredService<MockFileContent>();
        return content.OpenRead(localFile);
    }
}