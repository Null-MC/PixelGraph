using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Importing;
using PixelGraph.Common.ResourcePack;
using PixelGraph.UI.ViewModels;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PixelGraph.UI.Windows
{
    public partial class ImportPackWindow
    {
        private readonly IServiceProvider provider;


        public ImportPackWindow(IServiceProvider provider)
        {
            this.provider = provider;

            InitializeComponent();
        }

        private ImportTreeNode GetPathNode(IInputReader reader, string localPath)
        {
            var node = new ImportTreeDirectory {
                Name = Path.GetFileName(localPath),
                Path = localPath,
            };

            foreach (var childPath in reader.EnumerateDirectories(localPath, "*")) {
                var childNode = GetPathNode(reader, childPath);
                node.Nodes.Add(childNode);
            }

            foreach (var file in reader.EnumerateFiles(localPath, "*.*")) {
                var fileName = Path.GetFileName(file);

                var childNode = new ImportTreeFile {
                    Name = fileName,
                    Filename = file,
                };

                node.Nodes.Add(childNode);
            }

            return node;
        }

        private async Task RunAsync(CancellationToken token = default)
        {
            await using var scope = BuildScope();
            var reader = scope.GetRequiredService<IInputReader>();
            var writer = scope.GetRequiredService<IOutputWriter>();
            var importer = scope.GetRequiredService<IResourcePackImporter>();

            reader.SetRoot(VM.ImportSource);
            writer.SetRoot(VM.RootDirectory);

            importer.AsGlobal = VM.AsGlobal;
            importer.CopyUntracked = VM.CopyUntracked;
            importer.PackInput = VM.PackInput;

            importer.PackProfile = new ResourcePackProfileProperties {
                Output = new ResourcePackOutputProperties {
                    Format = VM.SourceFormat,
                    //...
                },
            };

            await importer.ImportAsync(token);

            await Application.Current.Dispatcher.BeginInvoke(() => {
                MessageBox.Show("Import completed successfully.", "Import Complete.");
            });

            Close();
        }

        private ServiceProvider BuildScope()
        {
            var scopeBuilder = provider.GetRequiredService<IServiceBuilder>();
            scopeBuilder.AddFileOutput();

            if (VM.IsArchive) scopeBuilder.AddArchiveInput();
            else scopeBuilder.AddFileInput();

            return scopeBuilder.Build();
        }

        #region Events

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            await using var scope = BuildScope();
            var reader = scope.GetRequiredService<IInputReader>();

            reader.SetRoot(VM.ImportSource);

            VM.RootNode = GetPathNode(reader, ".");
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void OnImportClick(object sender, RoutedEventArgs e)
        {
            try {
                await RunAsync();
            }
            catch (Exception error) {
                Application.Current.Dispatcher.Invoke(() => {
                    MessageBox.Show(error.Message, "Import Failed!");
                });
            }
        }

        #endregion
    }
}
