using PixelGraph.UI.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace PixelGraph.UI.Controls
{
    public partial class DocumentTabControl
    {
        public event EventHandler<CloseTabEventArgs> CloseTab;
        public event EventHandler CloseAllTabs;


        public DocumentTabControl()
        {
            InitializeComponent();
        }

        private void OnPreviewTabPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not MainWindowViewModel model) return;

            model.IsPreviewTabSelected = true;
            model.TabListSelection = null;
        }

        private void OnContextMenuCloseTabClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel {HasSelectedTab: true} model)
                OnCloseTab(model.SelectedTab.Id);
        }

        private void OnContextMenuCloseAllClick(object sender, RoutedEventArgs e)
        {
            OnCloseAllTabs();
        }

        private void OnCloseTab(Guid tabId)
        {
            var e = new CloseTabEventArgs(tabId);
            CloseTab?.Invoke(this, e);
        }

        private void OnCloseAllTabs()
        {
            CloseAllTabs?.Invoke(this, EventArgs.Empty);
        }
    }

    public class CloseTabEventArgs : EventArgs
    {
        public Guid TabId {get;}

        public CloseTabEventArgs(Guid tabId)
        {
            TabId = tabId;
        }
    }
}
