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
            var builder = provider.GetRequiredService<IServiceBuilder>();

            builder.AddContentReader(ContentTypes.File);
            builder.AddTextureReader(GameEditions.None);

            builder.AddContentWriter(Model.Archive ? ContentTypes.Archive : ContentTypes.File);
            builder.AddTextureWriter(GameEdition.Parse(Model.Profile.Edition));

            //if (GameEditions.Is(profile.Edition, GameEditions.Java)) {
            //    //return provider.GetRequiredService<IJavaPublisher>();
            //}
            //else if (GameEditions.Is(profile.Edition, GameEditions.Bedrock)) {
            //    //return provider.GetRequiredService<IBedrockPublisher>();
            //}
            //else throw new ApplicationException($"Unsupported game edition '{profile.Edition}'!");
            
            var logReceiver = builder.AddLoggingRedirect();
            logReceiver.LogMessage += OnInternalLog;

            await using var scope = builder.Build();
            var reader = scope.GetRequiredService<IInputReader>();
            var writer = scope.GetRequiredService<IOutputWriter>();

            reader.SetRoot(Model.RootDirectory);
            writer.SetRoot(Model.Destination);

            OnLogAppended(LogLevel.None, "Preparing output directory...");
            writer.Prepare();

            var context = new ResourcePackContext {
                Input = Model.Input,
                Profile = Model.Profile,
            };

            OnLogAppended(LogLevel.None, "Publishing content...");
            var publisher = GetPublisher(scope, context.Profile);

            //if (writer.AllowConcurrency)
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
                return provider.GetRequiredService<IJavaPublisher>();

            if (GameEdition.Is(profile.Edition, GameEdition.Bedrock))
                return provider.GetRequiredService<IBedrockPublisher>();

            throw new ApplicationException($"Unsupported game edition '{profile.Edition}'!");
        }
    }
}
