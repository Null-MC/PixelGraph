using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using MinecraftMappings.Minecraft.Java;
using Ookii.Dialogs.Wpf;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.Models;
using PixelGraph.UI.ViewModels;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PixelGraph.UI.Windows
{
    public partial class NewProjectWindow
    {
        private readonly NewProjectViewModel viewModel;


        public NewProjectWindow(IServiceProvider provider)
        {
            var themeHelper = provider.GetRequiredService<IThemeHelper>();

            InitializeComponent();
            themeHelper.ApplyCurrent(this);

            viewModel = new NewProjectViewModel(provider) {
                Model = Model,
            };
        }

        public async Task BuildProjectAsync(CancellationToken token = default)
        {
            var projectContext = BuildProjectContext();
            await viewModel.CreateProjectAsync(projectContext, token);
        }

        private ProjectContext BuildProjectContext()
        {
            var projectContext = new ProjectContext {
                Project = new ProjectData {
                    Name = Model.PackName,
                    Input = new ResourcePackInputProperties {
                        Format = TextureFormat.Format_Raw,
                    },
                },
                ProjectFilename = Model.ProjectFilename,
                RootDirectory = Path.GetDirectoryName(Model.ProjectFilename),
            };

            var packProfile = new ResourcePackProfileProperties {
                Name = $"{Model.PackName}-LabPbr",
                Description = "A short description of the RP content.",
                Encoding = {
                    Format = TextureFormat.Format_Lab13,
                },
                Edition = "Java",
                Format = JavaPackVersion.Latest.Index,
            };

            projectContext.Project.Profiles.Add(packProfile);
            projectContext.SelectedProfile = packProfile;

            return projectContext;
        }

        #region Events

        private void OnLocationBrowseClick(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaSaveFileDialog {
                FileName = "project.yml",
                Filter = "Project Yaml|*.yml;*.yaml|All Files|*.*",
            };

            if (dialog.ShowDialog(this) != true) return;
            Model.ProjectFilename = dialog.FileName;
        }

        private async void OnLocationNextClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Model.ProjectFilename)) {
                await this.ShowMessageAsync("Error!", "You must select a project file first!");
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

            DialogResult = true;
        }

        #endregion
    }
}
