using System.Windows.Media;

namespace PixelGraph.UI.ViewModels
{
    internal class ProfileItem
    {
        public string Name {get; set;}
        public string LocalFile {get; set;}
    }

    public class LogMessageItem
    {
        public string Message {get; set;}
        public Brush Color {get; set;}
    }

    internal interface ISearchParameters
    {
        string SearchText {get;}
        bool ShowAllFiles {get;}
    }
}
