using System.Windows.Media;

namespace PixelGraph.UI.ViewModels
{
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
