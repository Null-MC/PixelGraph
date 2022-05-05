using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Common.Projects;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Extensions;
using PixelGraph.UI.Internal.Settings;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.ViewModels
{
    internal class PublishOutputViewModel : ModelBase, IDisposable
    {
        private readonly CancellationTokenSource tokenSource;
        private ILogger<PublishOutputViewModel> logger;
        private IServiceProvider _provider;
        private IAppSettings settings;
        private IProjectContextManager projectContextMgr;
        private double _progress;
        private bool _closeOnComplete;
        private bool isInitializing;

        private volatile bool isRunning;
        private volatile bool _isLoading;
        private volatile bool _isActive;
        private volatile bool _isAnalyzing;

        public event EventHandler<PublishStatus> StateChanged;
        public event EventHandler<LogEventArgs> LogAppended;

        public string Destination {get; set;}
        public bool Archive {get; set;}
        public bool Clean {get; set;}

        public bool IsLoading {
            get => _isLoading;
            set {
                if (_isLoading == value) return;
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public bool IsActive {
            get => _isActive;
            set {
                if (_isActive == value) return;
                _isActive = value;
                OnPropertyChanged();
            }
        }

        public bool IsAnalyzing {
            get => _isAnalyzing;
            set {
                if (_isAnalyzing == value) return;
                _isAnalyzing = value;
                OnPropertyChanged();
            }
        }

        public double Progress {
            get => _progress;
            set {
                if (Math.Abs(_progress - value) < float.Epsilon) return;
                _progress = value;
                OnPropertyChanged();
            }
        }

        public bool CloseOnComplete {
            get => _closeOnComplete;
            set {
                if (_closeOnComplete == value) return;
                _closeOnComplete = value;
                OnPropertyChanged();

                if (!isInitializing) {
                    settings.Data.PublishCloseOnComplete = value;
                    settings.SaveAsync();
                }
            }
        }


        public PublishOutputViewModel()
        {
            tokenSource = new CancellationTokenSource();
            isInitializing = true;
        }

        public void Initialize(IServiceProvider provider)
        {
            _provider = provider;

            logger = provider.GetRequiredService<ILogger<PublishOutputViewModel>>();
            projectContextMgr = provider.GetRequiredService<IProjectContextManager>();
            settings = provider.GetRequiredService<IAppSettings>();

            isInitializing = false;
        }

        public void Dispose()
        {
            tokenSource?.Dispose();
        }

        public async Task<bool> PublishAsync(CancellationToken token = default)
        {
            isRunning = true;

            var status = new PublishStatus {
                IsAnalyzing = true,
                Progress = 0d,
            };

            OnStateChanged(ref status);

            var projectContext = projectContextMgr.GetContext();

            var timer = Stopwatch.StartNew();
            logger.LogInformation("Publishing profile '{Name}'...", projectContext.SelectedProfile.Name);

            var concurrency = settings.Data.Concurrency ?? ConcurrencyHelper.GetDefaultValue();
            OnLogAppended(LogLevel.Debug, $"  Concurrency: {concurrency:N0}");
            IPublishSummary summary = null;

            try {
                summary = await Task.Run(() => PublishInternalAsync(projectContext, token), token);
                timer.Stop();

                logger.LogInformation("Publish successful. Duration: {Elapsed}", timer.Elapsed);
                OnLogAppended(LogLevel.None, "Publish completed successfully.");
                return true;
            }
            catch (OperationCanceledException) {
                logger.LogWarning("Publish cancelled.");
                OnLogAppended(LogLevel.Warning, "Publish Cancelled!");
                throw;
            }
            catch (Exception error) {
                logger.LogError(error, "Failed to publish resource pack!");
                OnLogAppended(LogLevel.Error, $"Publish Failed! {error.UnfoldMessageString()}");
                return false;
            }
            finally {
                isRunning = false;
                timer.Stop();

                OnLogAppended(LogLevel.Debug, $"Duration    : {UnitHelper.GetReadableTimespan(timer.Elapsed)}");
                if (summary != null) {
                    OnLogAppended(LogLevel.Debug, $"# Materials : {summary.MaterialCount:N0}");
                    OnLogAppended(LogLevel.Debug, $"# Textures  : {summary.TextureCount:N0}");
                    OnLogAppended(LogLevel.Debug, $"Disk Size   : {summary.DiskSize}");
                    OnLogAppended(LogLevel.Debug, $"Tex Memory  : {summary.RawSize}");
                }
            }
        }

        public void Cancel()
        {
            if (!isRunning) return;

            OnLogAppended(LogLevel.Warning, "Cancelling...");
            tokenSource?.Cancel();
        }

        private async Task<IPublishSummary> PublishInternalAsync(ProjectContext projectContext, CancellationToken token)
        {
            var serviceBuilder = _provider.GetRequiredService<IServiceBuilder>();

            var edition = GameEdition.Parse(projectContext.SelectedProfile.Edition);
            var contentType = Archive ? ContentTypes.Archive : ContentTypes.File;

            serviceBuilder.Initialize();
            serviceBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);
            serviceBuilder.ConfigureWriter(contentType, edition, Destination);
            serviceBuilder.AddPublisher(edition);

            var logReceiver = serviceBuilder.AddSerilogRedirect();
            logReceiver.LogMessage += OnInternalLog;
            
            await using var scope = serviceBuilder.Build();

            OnLogAppended(LogLevel.None, "Preparing output directory...");
            var writer = scope.GetRequiredService<IOutputWriter>();
            writer.Prepare();

            var context = new ProjectPublishContext {
                Project = projectContext.Project,
                Profile = projectContext.SelectedProfile,
            };

            var publisher = scope.GetRequiredService<IPublisher>();
            publisher.Concurrency = settings.Data.Concurrency ?? ConcurrencyHelper.GetDefaultValue();
            publisher.StateChanged += (_, e) => OnStateChanged(ref e);

            OnLogAppended(LogLevel.None, "Analyzing content...");
            await publisher.PrepareAsync(context, Clean, token);

            var status = new PublishStatus {
                IsAnalyzing = false,
                Progress = 0d,
            };

            OnStateChanged(ref status);

            OnLogAppended(LogLevel.None, "Publishing content...");

            await publisher.PublishAsync(context, token);

            return scope.GetRequiredService<IPublishSummary>();
        }

        private void OnInternalLog(object sender, LogEventArgs e)
        {
            OnLogAppended(e.Level, e.Message);
        }

        private void OnStateChanged(ref PublishStatus status)
        {
            StateChanged?.Invoke(this, status);
        }

        private void OnLogAppended(LogLevel level, string message)
        {
            var e = new LogEventArgs(level, message);
            LogAppended?.Invoke(this, e);
        }
    }

    internal class PublishOutputDesignerViewModel : PublishOutputViewModel
    {
        public PublishOutputDesignerViewModel()
        {
            IsActive = true;
            //OnAppendLog(LogLevel.Debug, "Hello World!");
            //AppendLog(LogLevel.Warning, "Something is wrong...");
            //AppendLog(LogLevel.Error, "DANGER Will Robinson");
        }
    }
}
