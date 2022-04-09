using PixelGraph.UI.Models;
using System.Windows;
using System.Windows.Input;

namespace PixelGraph.UI.Windows
{
    public partial class TextureFormatWindow
    {
        public TextureFormatWindow()
        {
            InitializeComponent();
        }

        private void OnDataGridKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete) return;
            if (TextureEncodingDataGrid.SelectedValue is not TextureChannelMapping channel) return;

            channel.Clear();
        }

        //private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        //{
        //    DialogResult = false;
        //}

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
