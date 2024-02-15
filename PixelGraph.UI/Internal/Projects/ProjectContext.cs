using PixelGraph.Common.Projects;

namespace PixelGraph.UI.Internal.Projects;

public interface IProjectContext
{
    public string? ProjectFilename {get;}
    public string? RootDirectory {get;}

    public ProjectData? Project {get; set;}
    public PublishProfileProperties? SelectedProfile {get; set;}
}

public class ProjectContext : IProjectContext
{
    private readonly object lockHandle = new();
    private PublishProfileProperties? _selectedProfile;
    private ProjectData? _project;

    public string? ProjectFilename {get; set;}
    public string? RootDirectory {get; set;}

    public ProjectData? Project {
        get {
            lock (lockHandle) return _project;
        }
        set {
            lock (lockHandle) _project = value;
        }
    }

    public PublishProfileProperties? SelectedProfile {
        get {
            lock (lockHandle) return _selectedProfile;
        }
        set {
            lock (lockHandle) _selectedProfile = value;
        }
    }
}