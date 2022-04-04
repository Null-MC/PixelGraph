namespace PixelGraph.UI.Internal
{
    public interface IProjectContext
    {
        string RootDirectory {get; set;}
    }

    internal class ProjectContext : IProjectContext
    {
        public string RootDirectory {get; set;}
    }
}
