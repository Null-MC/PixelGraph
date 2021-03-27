using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PixelGraph.UI.Internal
{
    internal interface IRecentPathManager
    {
        ObservableCollection<string> List {get;}

        Task InitializeAsync(CancellationToken token = default);
        Task InsertAsync(string path, CancellationToken token = default);
        Task RemoveAsync(string path, CancellationToken token = default);
    }

    internal class RecentPathManager : IRecentPathManager
    {
        private const int MaxCount = 8;
        private const string FileName = "Recent.txt";

        private readonly IAppDataUtility appData;

        public ObservableCollection<string> List {get;}


        public RecentPathManager(IAppDataUtility appData)
        {
            this.appData = appData;

            List = new ObservableCollection<string>();
        }

        public async Task InitializeAsync(CancellationToken token = default)
        {
            List.Clear();

            var lines = await appData.ReadLinesAsync(FileName, token);
            if (lines == null) return;

            await Application.Current.Dispatcher.InvokeAsync(() => {
                foreach (var line in lines) List.Add(line);
            });
        }

        public async Task InsertAsync(string path, CancellationToken token = default)
        {
            await Application.Current.Dispatcher.InvokeAsync(() => Insert(path));
            await SaveAsync(token);
        }

        public async Task RemoveAsync(string path, CancellationToken token = default)
        {
            await Application.Current.Dispatcher.InvokeAsync(() => Remove(path));
            await SaveAsync(token);
        }

        private void Insert(string path)
        {
            Remove(path);
            List.Insert(0, path);

            for (var i = List.Count - 1; i >= MaxCount; i--)
                List.RemoveAt(i);
        }

        private void Remove(string path)
        {
            var t = Math.Min(List.Count, MaxCount);

            for (var i = t - 1; i >= 0; i--) {
                if (string.Equals(List[i], path))
                    List.RemoveAt(i);
            }
        }

        private Task SaveAsync(CancellationToken token)
        {
            return appData.WriteLinesAsync(FileName, List, token);
        }
    }
}
