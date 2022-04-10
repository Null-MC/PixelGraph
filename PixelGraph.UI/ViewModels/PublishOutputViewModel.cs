using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Common.ResourcePack;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Extensions;
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Models;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.ViewModels
{
    internal class PublishOutputViewModel : IDisposable
    {
        private readonly ILogger<PublishOutputViewModel> logger;
        private readonly IServiceProvider provider;
        private readonly IAppSettings settings;
        private readonly CancellationTokenSource tokenSource;
        private PublishOutputModel _model;
        private volatile bool isRunning;

        public event EventHandler<PublishStatus> StateChanged;
        public event EventHandler<LogEventArgs> LogAppended;

        public PublishOutputModel Model {
            get => _model;
            set {
                if (_model != value) SetModel(value);
            }
        }


        public PublishOutputViewModel(IServiceProvider provider)
        {
            this.provider = provider;

            settings = provider.GetRequiredService<IAppSettings>();
            logger = provider.GetRequiredService<ILogger<PublishOutputViewModel>>();

            tokenSource = new CancellationTokenSource();
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

            var timer = Stopwatch.StartNew();
            logger.LogInformation("Publishing profile '{Name}'...", Model.Profile.Name);

            var concurrency = settings.Data.Concurrency ?? ConcurrencyHelper.GetDefaultValue();
            OnLogAppended(LogLevel.Debug, $"  Concurrency: {concurrency:N0}");
            IPublishSummary summary = null;

            try {
                summary = await Task.Run(() => PublishInternalAsync(token), token);
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

        private async Task<IPublishSummary> PublishInternalAsync(CancellationToken token)
        {
            var appSettings = provider.GetRequiredService<IAppSettings>();
            var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();

            var edition = GameEdition.Parse(Model.Profile.Edition);
            var contentType = Model.Archive ? ContentTypes.Archive : ContentTypes.File;

            serviceBuilder.Initialize();
            serviceBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, Model.RootDirectory);
            serviceBuilder.ConfigureWriter(contentType, edition, Model.Destination);
            serviceBuilder.AddPublisher(edition);

            var logReceiver = serviceBuilder.AddSerilogRedirect();
            logReceiver.LogMessage += OnInternalLog;
            
            await using var scope = serviceBuilder.Build();

            OnLogAppended(LogLevel.None, "Preparing output directory...");
            var writer = scope.GetRequiredService<IOutputWriter>();
            writer.Prepare();

            var context = new ResourcePackContext {
                Input = Model.Input,
                Profile = Model.Profile,
            };

            var publisher = scope.GetRequiredService<IPublisher>();
            publisher.Concurrency = appSettings.Data.Concurrency ?? ConcurrencyHelper.GetDefaultValue();
            publisher.StateChanged += (_, e) => OnStateChanged(ref e);

            OnLogAppended(LogLevel.None, "Analyzing content...");
            await publisher.PrepareAsync(context, Model.Clean, token);

            var status = new PublishStatus {
                IsAnalyzing = false,
                Progress = 0d,
            };

            OnStateChanged(ref status);

            OnLogAppended(LogLevel.None, "Publishing content...");

            await publisher.PublishAsync(context, token);

            return scope.GetRequiredService<IPublishSummary>();
        }

        private void SetModel(PublishOutputModel newModel)
        {
            if (_model != null) {
                _model.PropertyChanged -= OnModelPropertyChanged;
            }

            _model = newModel;

            if (_model != null) {
                _model.PropertyChanged += OnModelPropertyChanged;
            }
        }

        private async void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, nameof(PublishOutputModel.CloseOnComplete))) {
                if (Model.IsLoading) return;

                settings.Data.PublishCloseOnComplete = Model.CloseOnComplete;
                await settings.SaveAsync();
            }
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
}
