using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ookii.Dialogs.Wpf;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.Models;
using PixelGraph.UI.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PixelGraph.UI.Windows
{
    public partial class NewProjectWindow
    {
        private readonly ILogger<NewProjectWindow> logger;
        private readonly NewProjectViewModel viewModel;


        public NewProjectWindow(IServiceProvider provider)
        {
            logger = provider.GetRequiredService<ILogger<NewProjectWindow>>();
            var themeHelper = provider.GetRequiredService<IThemeHelper>();

            InitializeComponent();
            themeHelper.ApplyCurrent(this);

            viewModel = new NewProjectViewModel(provider) {
                Model = Model,
            };
        }

        private async Task<bool> CreateDirectoryAsync()
        {
            try {
                viewModel.CreateDirectories();
            }
            catch (Exception error) {
                logger.LogError(error, $"Failed to create new project directory \"{Model.Location}\"!");
                Model.SetState(NewProjectStates.Location);
                await this.ShowMessageAsync("Error!", "Failed to create new project directory!");
                return false;
            }

            var existingFiles = Directory.EnumerateFiles(Model.Location, "*", SearchOption.AllDirectories);

            if (existingFiles.Any()) {
                var result = await this.ShowMessageAsync("Warning!", "There is existing content in the chosen directory! Are you sure you want to proceed?", MessageDialogStyle.AffirmativeAndNegative);
                if (result != MessageDialogResult.Affirmative) return false;
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

        //private void OnLocationCancelClick(object sender, RoutedEventArgs e)
        //{
        //    DialogResult = false;
        //}

        private async void OnLocationNextClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Model.Location)) {
                await this.ShowMessageAsync("Error!", "Please select a location for your project content!");
                return;
            }

            Model.SetState(NewProjectStates.Review);
        }

        private void OnReviewBackClick(object sender, RoutedEventArgs e)
        {
            Model.SetState(NewProjectStates.Location);
        }

        private async void OnReviewCreateClick(object sender, RoutedEventArgs e)
        {
            if (Model.EnablePackImport && !Model.ImportFromDirectory && !Model.ImportFromArchive) {
                await this.ShowMessageAsync("Error!", "Please select the type of source you would like to import project content from!");
                return;
            }

            if (!await CreateDirectoryAsync()) return;

            await viewModel.CreatePackFilesAsync();

            await Dispatcher.BeginInvoke(() => DialogResult = true);
        }

        #endregion
    }
}
