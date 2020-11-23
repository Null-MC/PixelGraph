using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace PixelGraph.UI.ViewModels
{
    internal class PublishWindowVM : ViewModelBase, IDisposable
    {
        private CancellationTokenSource tokenSource;
        private List<ProfileItem> _profiles;
        private ProfileItem _selectedItem;
        private string _rootDirectory;
        private bool _archive, _clean;
        private volatile bool _isBusy;

        public bool IsBusy => _isBusy;
        public bool IsNotBusy => !_isBusy;
        public ObservableCollection<string> OutputLog {get;}

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
            OutputLog = new ObservableCollection<string>();
        }

        public bool PublishBegin()
        {
            if (_isBusy) return false;
            tokenSource?.Dispose();

            OutputLog.Clear();
            tokenSource = new CancellationTokenSource();
            _isBusy = true;
            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(IsNotBusy));
            return true;
        }

        public void PublishEnd()
        {
            _isBusy = false;
            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(IsNotBusy));
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

            PublishBegin();

            OutputLog.Add("Hello World!");
            OutputLog.Add("This is the second line...");
        }
    }
}
