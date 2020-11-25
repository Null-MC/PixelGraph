using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Media;

namespace PixelGraph.UI.ViewModels
{
    internal class PublishWindowVM : ViewModelBase, IDisposable
    {
        private CancellationTokenSource tokenSource;
        private List<ProfileItem> _profiles;
        private ProfileItem _selectedItem;
        private string _rootDirectory;
        private bool _archive, _clean;
        private volatile bool _showOutput, _isActive;

        public ObservableCollection<LogMessageItem> OutputLog {get;}

        public bool ShowOutput {
            get => _showOutput;
            set {
                _showOutput = value;
                OnPropertyChanged();
            }
        }

        public bool IsActive {
            get => _isActive;
            set {
                _isActive = value;
                OnPropertyChanged();
            }
        }

        public string RootDirectory {
            get => _rootDirectory;
            set {
                _rootDirectory = value;
                OnPropertyChanged();
            }
        }

        public List<ProfileItem> Profiles {
            get => _profiles;
            set {
                _profiles = value;
                OnPropertyChanged();
            }
        }

        public ProfileItem SelectedItem {
            get => _selectedItem;
            set {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        public bool Archive {
            get => _archive;
            set {
                _archive = value;
                OnPropertyChanged();
            }
        }

        public bool Clean {
            get => _clean;
            set {
                _clean = value;
                OnPropertyChanged();
            }
        }


        public PublishWindowVM()
        {
            OutputLog = new ObservableCollection<LogMessageItem>();
        }

        public bool PublishBegin(out CancellationToken token)
        {
            if (_isActive) {
                token = default;
                return false;
            }

            tokenSource?.Dispose();

            OutputLog.Clear();
            tokenSource = new CancellationTokenSource();
            ShowOutput = IsActive = true;
            token = tokenSource.Token;
            return true;
        }

        public void PublishEnd()
        {
            IsActive = false;
            tokenSource?.Dispose();
            tokenSource = null;
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
            Profiles = new List<ProfileItem> {
                new ProfileItem {
                    Name = "Profile A",
                },
                new ProfileItem {
                    Name = "Profile B",
                },
            };

            PublishBegin(out _);

            OutputLog.Add(new LogMessageItem {
                Message = "Hello World!",
                Color = new SolidColorBrush(Colors.Red),
            });
            OutputLog.Add(new LogMessageItem {
                Message = "This is the second line...",
                Color = new SolidColorBrush(Colors.Blue),
            });
        }
    }
}
