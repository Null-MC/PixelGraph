using Microsoft.Extensions.Logging;
using PixelGraph.UI.Internal;
using PixelGraph.UI.ViewData;
using System;
using System.Threading;

namespace PixelGraph.UI.ViewModels
{
    internal class PublishWindowVM : ViewModelBase, IDisposable
    {
        private readonly CancellationTokenSource tokenSource;
        private volatile bool _isActive;
        private bool _closeOnComplete;

        public event EventHandler<LogEventArgs> LogEvent;

        public ProfileItem Profile {get; set;}
        public string RootDirectory {get; set;}
        public string Destination {get; set;}
        public bool Archive {get; set;}
        public bool Clean {get; set;}

        public CancellationToken Token => tokenSource.Token;

        public bool IsActive {
            get => _isActive;
            set {
                _isActive = value;
                OnPropertyChanged();
            }
        }

        public bool CloseOnComplete {
            get => _closeOnComplete;
            set {
                _closeOnComplete = value;
                OnPropertyChanged();
            }
        }


        public PublishWindowVM()
        {
            tokenSource = new CancellationTokenSource();
        }

        public void Cancel()
        {
            tokenSource?.Cancel();
        }

        public void Dispose()
        {
            tokenSource?.Dispose();
        }

        public void AppendLog(LogLevel level, string text)
        {
            var e = new LogEventArgs(level, text);
            LogEvent?.Invoke(this, e);
        }
    }

    internal class PublishWindowDesignVM : PublishWindowVM
    {
        public PublishWindowDesignVM()
        {
            IsActive = true;
            AppendLog(LogLevel.Debug, "Hello World!");
            AppendLog(LogLevel.Warning, "Something is wrong...");
            AppendLog(LogLevel.Error, "DANGER Will Robinson");
        }
    }
}
