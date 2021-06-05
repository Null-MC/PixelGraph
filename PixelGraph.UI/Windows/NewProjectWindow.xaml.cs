using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ookii.Dialogs.Wpf;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.Models;
using PixelGraph.UI.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace PixelGraph.UI.Windows
{
    public partial class NewProjectWindow
    {
        //private readonly IServiceProvider provider;
        private readonly ILogger<NewProjectWindow> logger;
        private readonly NewProjectViewModel viewModel;


        public NewProjectWindow(IServiceProvider provider)
        {
            //this.provider = provider;

            logger = provider.GetRequiredService<ILogger<NewProjectWindow>>();
            var themeHelper = provider.GetRequiredService<IThemeHelper>();

            InitializeComponent();
            themeHelper.ApplyCurrent(this);

            viewModel = new NewProjectViewModel(provider) {
                Model = Model,
            };
        }

        private bool CreateDirectory()
        {
            try {
                viewModel.CreateDirectories();
            }
            catch (Exception error) {
                logger.LogError(error, $"Failed to create new project directory \"{Model.Location}\"!");
                MessageBox.Show(this, "Failed to create new project directory!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                Model.SetState(NewProjectStates.Location);
                return false;
            }

            var existingFiles = Directory.EnumerateFiles(Model.Location, "*", SearchOption.AllDirectories);

            if (existingFiles.Any()) {
                var result = MessageBox.Show(this, "There is existing content in the chosen directory! Are you sure you want to proceed?", "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.No) return false;
            }

            return true;
        }

        #region Events

        private void OnLocationBrowseClick(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog {
                Description = "Please select a folder.",
                UseDescriptionForTitle = true,
            };

            if (dialog.ShowDialog(this) != true) return;
            Model.Location = dialog.SelectedPath;
        }

        private void OnLocationCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OnLocationNextClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Model.Location)) {
                MessageBox.Show(this, "Please select a location for your project content!", "Error!", MessageBoxButton.OK);
                return;
            }

            Model.SetState(NewProjectStates.Review);
        }

        //private void OnFormatBackClick(object sender, RoutedEventArgs e)
        //{
        //    VM.SetState(NewProjectStates.Location);
        //}

        //private void OnFormatNextClick(object sender, RoutedEventArgs e)
        //{
        //    VM.SetState(NewProjectStates.Review);
        //}

        private void OnReviewBackClick(object sender, RoutedEventArgs e)
        {
            Model.SetState(NewProjectStates.Location);
        }

        private async void OnReviewCreateClick(object sender, RoutedEventArgs e)
        {
            if (Model.EnablePackImport && !Model.ImportFromDirectory && !Model.ImportFromArchive) {
                MessageBox.Show(this, "Please select the type of source you would like to import project content from!", "Error!", MessageBoxButton.OK);
                return;
            }

            if (!CreateDirectory()) return;

            await viewModel.CreatePackFilesAsync();

            DialogResult = true;
        }

        #endregion
    }
}
