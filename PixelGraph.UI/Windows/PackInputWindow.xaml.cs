using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Utilities;
using System;
using System.Windows;

namespace PixelGraph.UI.Windows
{
    public partial class PackInputWindow
    {
        //private readonly ILogger<PackInputWindow> logger;
        private readonly IProjectContextManager projectContextMgr;
        //private readonly PackInputViewModel viewModel;


        public PackInputWindow(IServiceProvider provider)
        {
            //logger = provider.GetRequiredService<ILogger<PackInputWindow>>();
            projectContextMgr = provider.GetRequiredService<IProjectContextManager>();
            var themeHelper = provider.GetRequiredService<IThemeHelper>();

            InitializeComponent();
            themeHelper.ApplyCurrent(this);

            var projectContext = projectContextMgr.GetContext();
            Model.PackInput = (ResourcePackInputProperties)projectContext.Project.Input.Clone();

            //viewModel = new PackInputViewModel(provider) {
            //    Model = Model,
            //};
        }

        //private void ShowError(string message)
        //{
        //    Dispatcher.Invoke(() => {
        //        MessageBox.Show(this, message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
        //    });
        //}

        #region Events

        //private void OnDataGridKeyUp(object sender, KeyEventArgs e)
        //{
        //    if (e.Key != Key.Delete) return;
        //    if (EncodingDataGrid.SelectedValue is not TextureChannelMapping channel) return;

        //    channel.Clear();
        //}

        private void OnEditEncodingClick(object sender, RoutedEventArgs e)
        {
            var formatFactory = TextureFormat.GetFactory(Model.Format);

            var window = new TextureFormatWindow {
                Owner = this,
                Model = {
                    Encoding = (ResourcePackEncoding)Model.PackInput.Clone(),
                    DefaultEncoding = formatFactory.Create(),
                    //TextureFormat = Model.SourceFormat,
                    EnableSampler = false,
                    //DefaultSampler = Samplers.Nearest,
                },
            };

            if (window.ShowDialog() != true) return;

            Model.PackInput = (ResourcePackInputProperties)window.Model.Encoding;
            //Model.SourceFormat = window.Model.TextureFormat;
        }

        //private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        //{
        //    DialogResult = false;
        //}

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            var projectContext = projectContextMgr.GetContext();

            projectContext.Project.Input = Model.PackInput;
            DialogResult = true;
        }

        #endregion
    }
}
