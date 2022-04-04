using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Common.ResourcePack;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Extensions;
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Models;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.ViewModels
{
    internal class PublishOutputViewModel : IDisposable
    {
        private readonly IServiceProvider provider;
        private readonly IAppSettings settings;
        private readonly CancellationTokenSource tokenSource;
        private PublishOutputModel _model;
        private volatile bool isRunning;

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

            tokenSource = new CancellationTokenSource();
        }

        public void Dispose()
        {
            tokenSource?.Dispose();
        }

        public async Task PublishAsync()
        {
            isRunning = true;

            try {
                await PublishInternalAsync(tokenSource.Token);
            }
            finally {
                isRunning = false;
            }
        }

        public void Cancel()
        {
            if (!isRunning) return;

            OnLogAppended(LogLevel.Warning, "Cancelling...");
            tokenSource?.Cancel();
        }

        private async Task PublishInternalAsync(CancellationToken token)
        {
            var appSettings = provider.GetRequiredService<IAppSettings>();
            var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();

            var edition = GameEdition.Parse(Model.Profile.Edition);
            var contentType = Model.Archive ? ContentTypes.Archive : ContentTypes.File;

            serviceBuilder.Initialize();
            serviceBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, Model.RootDirectory);
            serviceBuilder.ConfigureWriter(contentType, edition, Model.Destination);

            var logReceiver = serviceBuilder.AddLoggingRedirect();
            logReceiver.LogMessage += OnInternalLog;

            await using var scope = serviceBuilder.Build();

            OnLogAppended(LogLevel.None, "Preparing output directory...");
            var writer = scope.GetRequiredService<IOutputWriter>();
            writer.Prepare();

            var context = new ResourcePackContext {
                //RootPath = Model.RootDirectory,
                Input = Model.Input,
                Profile = Model.Profile,
            };

            OnLogAppended(LogLevel.None, "Publishing content...");
            var publisher = GetPublisher(scope, context.Profile);
            publisher.Concurrency = appSettings.Data.Concurrency ?? Environment.ProcessorCount;

            await publisher.PublishAsync(context, Model.Clean, token);
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

        private void OnLogAppended(LogLevel level, string message)
        {
            var e = new LogEventArgs(level, message);
            LogAppended?.Invoke(this, e);
        }

        private static IPublisher GetPublisher(IServiceProvider provider, ResourcePackProfileProperties profile)
        {
            if (GameEdition.Is(profile.Edition, GameEdition.Java))
                return provider.GetRequiredService<JavaPublisher>();

            if (GameEdition.Is(profile.Edition, GameEdition.Bedrock))
                return provider.GetRequiredService<BedrockPublisher>();

            throw new ApplicationException($"Unsupported game edition '{profile.Edition}'!");
        }
    }
}
