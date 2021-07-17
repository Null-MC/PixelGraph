using PixelGraph.UI.Models;
using PixelGraph.UI.Models.Tabs;
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
            if (DataContext is not MainWindowModel model) return;

            model.IsPreviewTabSelected = true;
            model.TabListSelection = null;
        }

        private void OnContextMenuCloseTabClick(object sender, RoutedEventArgs e)
        {
            var tab = (ITabModel)Root.SelectedValue;
            if (tab != null) OnCloseTab(tab.Id);
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
