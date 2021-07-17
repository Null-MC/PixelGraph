using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Importing;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.ViewModels;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PixelGraph.UI.Windows
{
    public partial class ImportPackWindow : IDisposable
    {
        private readonly CancellationTokenSource tokenSource;
        private readonly IServiceProvider provider;
        private readonly ILogger logger;


        public ImportPackWindow(IServiceProvider provider)
        {
            this.provider = provider;

            logger = provider.GetRequiredService<ILogger<ImportPackWindow>>();
            tokenSource = new CancellationTokenSource();

            InitializeComponent();

            var themeHelper = provider.GetRequiredService<IThemeHelper>();
            themeHelper.ApplyCurrent(this);

            Model.LogEvent += OnLogListAppended;
        }

        private async Task LoadSourceAsync(CancellationToken token)
        {
            await using var scope = BuildScope();
            var reader = scope.GetRequiredService<IInputReader>();

            reader.SetRoot(Model.ImportSource);

            var root = await Task.Run(() => GetPathNode(reader, ".", token), token);

            await Dispatcher.BeginInvoke(() => {
                Model.RootNode = root;
                Model.IsReady = true;
            });
        }

        private async Task RunAsync(CancellationToken token)
        {
            var scopeBuilder = provider.GetRequiredService<IServiceBuilder>();
            scopeBuilder.AddFileOutput();

            if (Model.IsArchive) scopeBuilder.AddArchiveInput();
            else scopeBuilder.AddFileInput();

            var logReceiver = scopeBuilder.AddLoggingRedirect();
            logReceiver.LogMessage += OnLogMessage;

            await using var scope = scopeBuilder.Build();

            var reader = scope.GetRequiredService<IInputReader>();
            var writer = scope.GetRequiredService<IOutputWriter>();
            var importer = scope.GetRequiredService<IResourcePackImporter>();

            reader.SetRoot(Model.ImportSource);
            writer.SetRoot(Model.RootDirectory);

            importer.AsGlobal = Model.AsGlobal;
            importer.CopyUntracked = Model.CopyUntracked;
            importer.IncludeUnknown = Model.IncludeUnknown;
            importer.PackInput = Model.PackInput;

            Model.Encoding.Format = Model.SourceFormat;

            importer.PackProfile = new ResourcePackProfileProperties {
                Encoding = Model.Encoding,
            };

            await importer.ImportAsync(token);
        }

        private ServiceProvider BuildScope()
        {
            var scopeBuilder = provider.GetRequiredService<IServiceBuilder>();
            scopeBuilder.AddFileOutput();

            if (Model.IsArchive) scopeBuilder.AddArchiveInput();
            else scopeBuilder.AddFileInput();

            return scopeBuilder.Build();
        }

        private ImportTreeNode GetPathNode(IInputReader reader, string localPath, CancellationToken token)
        {
            var node = new ImportTreeDirectory {
                Name = Path.GetFileName(localPath),
                Path = localPath,
            };

            foreach (var childPath in reader.EnumerateDirectories(localPath, "*")) {
                token.ThrowIfCancellationRequested();

                if (!Model.IncludeUnknown && ResourcePackImporter.IsUnknownPath(childPath)) continue;
                
                var childNode = GetPathNode(reader, childPath, token);
                node.Nodes.Add(childNode);
            }

            foreach (var file in reader.EnumerateFiles(localPath, "*.*")) {
                token.ThrowIfCancellationRequested();

                if (!Model.IncludeUnknown && ResourcePackImporter.IsUnknownFile(file)) continue;

                var childNode = new ImportTreeFile {
                    Name = Path.GetFileName(file),
                    Filename = file,
                };

                node.Nodes.Add(childNode);
            }

            return node;
        }

        //private static void BeginMessageBox(string message, string title)
        //{
        //    Application.Current.Dispatcher.Invoke(() => {
        //        MessageBox.Show(message, title);
        //    });
        //}

        public void Dispose()
        {
            tokenSource?.Dispose();
        }

        #region Events

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            await LoadSourceAsync(tokenSource.Token);
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            tokenSource.Cancel();
        }

        private async void OnImportClick(object sender, RoutedEventArgs e)
        {
            Model.ShowLog = true;
            Model.IsActive = true;

            try {
                logger.LogInformation("Importing source '{ImportSource}'...", Model.ImportSource);

                await Task.Run(() => RunAsync(tokenSource.Token), tokenSource.Token);

                logger.LogInformation("Import successful.");
                LogList.Append(LogLevel.Information, "Import completed successfully.");
                DialogResult = true;
            }
            catch (OperationCanceledException) {
                logger.LogWarning("Import cancelled.");
                LogList.Append(LogLevel.Warning, "Operation Cancelled.");
                DialogResult = false;
            }
            catch (Exception error) {
                logger.LogError(error, "Import failed!");
                LogList.Append(LogLevel.Error, $"ERROR! {error.UnfoldMessageString()}");
            }
            finally {
                Model.IsActive = false;
            }
        }

        private void OnEditEncodingClick(object sender, RoutedEventArgs e)
        {
            var formatFactory = TextureFormat.GetFactory(Model.SourceFormat);

            var window = new TextureFormatWindow {
                Owner = this,
                Model = {
                    Encoding = (ResourcePackEncoding)Model.Encoding.Clone(),
                    DefaultEncoding = formatFactory.Create(),
                    //TextureFormat = Model.SourceFormat,
                    EnableSampler = false,
                    //DefaultSampler = Samplers.Nearest,
                },
            };

            if (window.ShowDialog() != true) return;

            Model.Encoding = (ResourcePackOutputProperties)window.Model.Encoding;
            //Model.SourceFormat = window.Model.TextureFormat;
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            tokenSource.Cancel();
            LogList.Append(LogLevel.Warning, "Cancelling...");

            //DialogResult = false;
            //Close();
        }

        private void OnLogMessage(object sender, LogEventArgs e)
        {
            LogList.Append(e.Level, e.Message);
        }

        private void OnLogListAppended(object sender, LogEventArgs e)
        {
            LogList.Append(e.Level, e.Message);
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #endregion

        private async void OnIncludeUnknownChanged(object sender, EventArgs e)
        {
            // TODO: cancel current load task

            Model.IsReady = false;

            await LoadSourceAsync(tokenSource.Token);
        }
    }
}
