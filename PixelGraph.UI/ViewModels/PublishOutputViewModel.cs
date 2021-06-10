using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.ResourcePack;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Settings;
using PixelGraph.UI.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.ViewModels
{
    internal class PublishOutputViewModel : IDisposable
    {
        private readonly IServiceProvider provider;
        private readonly IAppSettings settings;
        private readonly CancellationTokenSource tokenSource;

        public event EventHandler<LogEventArgs> LogAppended;

        public PublishOutputModel Model {get; set;}


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

        public async Task PublishAsync(CancellationToken token = default)
        {
            var builder = provider.GetRequiredService<IServiceBuilder>();
            builder.AddFileInput();

            if (Model.Archive) builder.AddArchiveOutput();
            else builder.AddFileOutput();

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
            var packReader = scope.GetRequiredService<IResourcePackReader>();

            reader.SetRoot(Model.RootDirectory);
            writer.SetRoot(Model.Destination);

            OnLogAppended(LogLevel.None, "Preparing output directory...");
            writer.Prepare();

            var context = new ResourcePackContext {
                Input = await packReader.ReadInputAsync("input.yml"),
                Profile = await packReader.ReadProfileAsync(Model.Profile.LocalFile),
                //UseGlobalOutput = true,
            };

            OnLogAppended(LogLevel.None, "Publishing content...");
            var publisher = GetPublisher(scope, context.Profile);
            await publisher.PublishAsync(context, Model.Clean, token);
        }

        public Task UpdateSettingsAsync(CancellationToken token = default)
        {
            settings.Data.PublishCloseOnComplete = Model.CloseOnComplete;

            return settings.SaveAsync(token);
        }

        public void Cancel()
        {
            tokenSource?.Cancel();
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
            if (GameEditions.Is(profile.Edition, GameEditions.Java))
                return provider.GetRequiredService<IJavaPublisher>();

            if (GameEditions.Is(profile.Edition, GameEditions.Bedrock))
                return provider.GetRequiredService<IBedrockPublisher>();

            throw new ApplicationException($"Unsupported game edition '{profile.Edition}'!");
        }
    }
}
