using PixelGraph.UI.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace PixelGraph.UI.ViewModels
{
    internal class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected static Visibility GetVisibility(bool value) => value ? Visibility.Visible : Visibility.Collapsed;
    }
}
