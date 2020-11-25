using System;
using System.Collections.ObjectModel;
using System.IO;
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
    }

    internal class RecentPathManager : IRecentPathManager
    {
        private const int MaxCount = 8;
        private const string AppName = "PixelGraph";
        private const string FileName = "Recent.txt";

        private readonly string _filename, _path;

        public ObservableCollection<string> List {get;}


        public RecentPathManager()
        {
            List = new ObservableCollection<string>();

            var rootPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _path = Path.Combine(rootPath, AppName);
            _filename = Path.Combine(_path, FileName);
        }

        public async Task InitializeAsync(CancellationToken token = default)
        {
            List.Clear();

            if (!File.Exists(_filename)) return;

            var lines = await File.ReadAllLinesAsync(_filename, token);

            await Application.Current.Dispatcher.InvokeAsync(() => {
                foreach (var line in lines) List.Add(line);
            });
        }

        public async Task InsertAsync(string path, CancellationToken token = default)
        {
            await Application.Current.Dispatcher.InvokeAsync(() => Insert(path));

            if (!Directory.Exists(_path)) Directory.CreateDirectory(_path);
            await File.WriteAllLinesAsync(_filename, List, token);
        }

        private void Insert(string path)
        {
            List.Insert(0, path);

            var t = Math.Min(List.Count, MaxCount);
            for (var i = t - 1; i > 0; i--) {
                if (string.Equals(List[i], path))
                    List.RemoveAt(i);
            }

            for (var i = List.Count - 1; i >= MaxCount; i--)
                List.RemoveAt(i);
        }
    }
}
