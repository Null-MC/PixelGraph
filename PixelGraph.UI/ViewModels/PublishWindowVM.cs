using Microsoft.Extensions.Logging;
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

        public ProfileItem Profile {get; set;}
        public string RootDirectory {get; set;}
        public string Destination {get; set;}
        public bool Archive {get; set;}
        public bool Clean {get; set;}
        public LogListVM LogList {get;}

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
            LogList = new LogListVM();
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
    }

    internal class PublishWindowDesignVM : PublishWindowVM
    {
        public PublishWindowDesignVM()
        {
            IsActive = true;
            LogList.Append(LogLevel.Debug, "Hello World!");
            LogList.Append(LogLevel.Warning, "Something is wrong...");
            LogList.Append(LogLevel.Error, "DANGER Will Robinson");
        }
    }
}
