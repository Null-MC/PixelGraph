using PixelGraph.UI.Models;
using System.Windows.Input;

namespace PixelGraph.UI.Controls
{
    public partial class DocumentTabControl
    {
        public DocumentTabControl()
        {
            InitializeComponent();
        }

        private void OnPreviewTabPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not MainWindowModel model) return;

            model.IsPreviewTabSelected = true;
            model.TabListSelection = null;
        }
    }
}
