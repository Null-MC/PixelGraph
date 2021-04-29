using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ookii.Dialogs.Wpf;
using PixelGraph.Common.Encoding;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.ResourcePack;
using PixelGraph.UI.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PixelGraph.UI.Windows
{
    public partial class NewProjectWindow : Window
    {
        private readonly IServiceProvider provider;
        private readonly ILogger logger;


        public NewProjectWindow(IServiceProvider provider)
        {
            this.provider = provider;
            logger = provider.GetRequiredService<ILogger<NewProjectWindow>>();

            InitializeComponent();
        }

        private bool CreateDirectory()
        {
            try {
                if (!Directory.Exists(VM.Location))
                    Directory.CreateDirectory(VM.Location);

                if (VM.CreateMinecraftFolders)
                    CreateDefaultMinecraftFolders();

                if (VM.CreateRealmsFolders)
                    CreateDefaultRealmsFolders();

                if (VM.CreateOptifineFolders)
                    CreateDefaultOptifineFolders();

                var existingFiles = Directory.EnumerateFiles(VM.Location, "*", SearchOption.AllDirectories);
                if (existingFiles.Any()) {
                    var result = MessageBox.Show(this, "There is existing content in the chosen directory! Are you sure you want to proceed?", "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                    if (result == MessageBoxResult.No) return false;
                }

                return true;
            }
            catch (Exception error) {
                logger.LogError(error, $"Failed to create new project directory \"{VM.Location}\"!");
                MessageBox.Show(this, "Failed to create new project directory!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                VM.SetState(NewProjectStates.Location);
                return false;
            }
        }

        private void CreateDefaultMinecraftFolders()
        {
            var mcPath = PathEx.Join(VM.Location, "assets", "minecraft");

            TryCreateDirectory(mcPath, "blockstates");
            TryCreateDirectory(mcPath, "font");
            TryCreateDirectory(mcPath, "models", "block");
            TryCreateDirectory(mcPath, "models", "item");
            TryCreateDirectory(mcPath, "particles");
            TryCreateDirectory(mcPath, "shaders");
            TryCreateDirectory(mcPath, "sounds");
            //TryCreateDirectory(mcPath, "sounds", "ambient");
            //TryCreateDirectory(mcPath, "sounds", "block");
            //TryCreateDirectory(mcPath, "sounds", "damage");
            //TryCreateDirectory(mcPath, "sounds", "dig");
            //TryCreateDirectory(mcPath, "sounds", "enchant");
            //TryCreateDirectory(mcPath, "sounds", "entity");
            //TryCreateDirectory(mcPath, "sounds", "fire");
            //TryCreateDirectory(mcPath, "sounds", "fireworks");
            //TryCreateDirectory(mcPath, "sounds", "item");
            //TryCreateDirectory(mcPath, "sounds", "liquid");
            //TryCreateDirectory(mcPath, "sounds", "minecart");
            //TryCreateDirectory(mcPath, "sounds", "mob");
            //TryCreateDirectory(mcPath, "sounds", "music");
            //TryCreateDirectory(mcPath, "sounds", "note");
            //TryCreateDirectory(mcPath, "sounds", "portal");
            //TryCreateDirectory(mcPath, "sounds", "random");
            //TryCreateDirectory(mcPath, "sounds", "records");
            //TryCreateDirectory(mcPath, "sounds", "step");
            //TryCreateDirectory(mcPath, "sounds", "tile");
            //TryCreateDirectory(mcPath, "sounds", "ui");
            TryCreateDirectory(mcPath, "texts");
            TryCreateDirectory(mcPath, "textures", "block");
            TryCreateDirectory(mcPath, "textures", "colormap");
            TryCreateDirectory(mcPath, "textures", "effect");
            TryCreateDirectory(mcPath, "textures", "entity");
            TryCreateDirectory(mcPath, "textures", "environment");
            TryCreateDirectory(mcPath, "textures", "font");
            TryCreateDirectory(mcPath, "textures", "gui");
            TryCreateDirectory(mcPath, "textures", "item");
            TryCreateDirectory(mcPath, "textures", "map");
            TryCreateDirectory(mcPath, "textures", "misc");
            TryCreateDirectory(mcPath, "textures", "mob_effect");
            TryCreateDirectory(mcPath, "textures", "models");
            TryCreateDirectory(mcPath, "textures", "painting");
            TryCreateDirectory(mcPath, "textures", "particle");

            TryCreateDirectory(mcPath, "optifine", "cit");
            TryCreateDirectory(mcPath, "optifine", "colormap");
            TryCreateDirectory(mcPath, "optifine", "ctm");
            TryCreateDirectory(mcPath, "optifine", "mob");
            TryCreateDirectory(mcPath, "optifine", "sky");

            var realmsPath = PathEx.Join(VM.Location, "assets", "realms");

            TryCreateDirectory(realmsPath, "textures");
        }

        private void CreateDefaultRealmsFolders()
        {
            var realmsPath = PathEx.Join(VM.Location, "assets", "realms");

            TryCreateDirectory(realmsPath, "textures");
        }

        private void CreateDefaultOptifineFolders()
        {
            var optifinePath = PathEx.Join(VM.Location, "assets", "minecraft", "optifine");

            TryCreateDirectory(optifinePath, "anim");
            TryCreateDirectory(optifinePath, "cem");
            TryCreateDirectory(optifinePath, "cit");
            TryCreateDirectory(optifinePath, "colormap");
            TryCreateDirectory(optifinePath, "ctm");
            TryCreateDirectory(optifinePath, "font");
            TryCreateDirectory(optifinePath, "gui");
            TryCreateDirectory(optifinePath, "lightmap");
            TryCreateDirectory(optifinePath, "mob");
            TryCreateDirectory(optifinePath, "random");
            TryCreateDirectory(optifinePath, "sky");
        }

        private void TryCreateDirectory(params string[] parts)
        {
            var path = PathEx.Join(parts);
            if (Directory.Exists(path)) return;
            Directory.CreateDirectory(path);
        }

        private async Task CreatePackFilesAsync()
        {
            var writer = provider.GetRequiredService<IOutputWriter>();
            var packWriter = provider.GetRequiredService<IResourcePackWriter>();
            writer.SetRoot(VM.Location);

            var packInput = new ResourcePackInputProperties {
                Format = VM.ContentFormat,
            };

            await packWriter.WriteAsync("input.yml", packInput);

            if (VM.CreateDefaultProfile) {
                var packProfile = new ResourcePackProfileProperties {
                    Description = "My new resource pack",
                    Encoding = {
                        Format = TextureEncoding.Format_Lab13,
                    },
                    Edition = "Java",
                    Format = 6,
                };

                await packWriter.WriteAsync("default.pack.yml", packProfile);
            }
        }

        #region Events

        private void OnLocationBrowseClick(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog {
                Description = "Please select a folder.",
                UseDescriptionForTitle = true,
            };

            if (dialog.ShowDialog(this) != true) return;
            VM.Location = dialog.SelectedPath;
        }

        private void OnLocationCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OnLocationNextClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(VM.Location)) {
                MessageBox.Show(this, "Please select a location for your project content!", "Error!", MessageBoxButton.OK);
                return;
            }

            VM.SetState(NewProjectStates.Format);
        }

        private void OnFormatBackClick(object sender, RoutedEventArgs e)
        {
            VM.SetState(NewProjectStates.Location);
        }

        private void OnFormatNextClick(object sender, RoutedEventArgs e)
        {
            VM.SetState(NewProjectStates.Review);
        }

        private void OnReviewBackClick(object sender, RoutedEventArgs e)
        {
            VM.SetState(NewProjectStates.Format);
        }

        private async void OnReviewCreateClick(object sender, RoutedEventArgs e)
        {
            if (VM.EnablePackImport && !VM.ImportFromDirectory && !VM.ImportFromArchive) {
                MessageBox.Show(this, "Please select the type of source you would like to import project content from!", "Error!", MessageBoxButton.OK);
                return;
            }

            if (!CreateDirectory()) return;

            await CreatePackFilesAsync();

            DialogResult = true;
        }

        #endregion
    }
}
