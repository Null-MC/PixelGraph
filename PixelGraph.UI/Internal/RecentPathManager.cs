using PixelGraph.UI.Internal.Utilities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.Internal
{
    internal interface IRecentPathManager
    {
        //List<string> List {get;}
        IEnumerable<string> Items {get;}

        Task LoadAsync(CancellationToken token = default);
        Task SaveAsync(CancellationToken token = default);
        //Task InsertAsync(string path, CancellationToken token = default);
        //Task RemoveAsync(string path, CancellationToken token = default);
        void Insert(string path);
        void Remove(string path);
    }

    internal class RecentPathManager : IRecentPathManager
    {
        private const int MaxCount = 8;
        private const string FileName = "Recent.txt";

        private readonly IAppDataUtility appData;
        private readonly List<string> _items;

        //public List<string> List {get;}
        public IEnumerable<string> Items => _items;


        public RecentPathManager(IAppDataUtility appData)
        {
            this.appData = appData;

            //List = new ObservableCollection<string>();
            _items = new List<string>();
        }

        public async Task LoadAsync(CancellationToken token = default)
        {
            //List.Clear();
            _items.Clear();

            var lines = await appData.ReadLinesAsync(FileName, token);
            if (lines == null) return;

            //await Application.Current.Dispatcher.InvokeAsync(() => {
            //    foreach (var line in lines) List.Add(line);
            //});
            _items.AddRange(lines);
        }

        public Task SaveAsync(CancellationToken token = default)
        {
            return appData.WriteLinesAsync(FileName, _items, token);
        }

        //public async Task InsertAsync(string path, CancellationToken token = default)
        //{
        //    //await Application.Current.Dispatcher.InvokeAsync(() => Insert(path));
        //    Insert(path);
        //    await SaveAsync(token);
        //}

        //public async Task RemoveAsync(string path, CancellationToken token = default)
        //{
        //    //await Application.Current.Dispatcher.InvokeAsync(() => Remove(path));
        //    _items.Remove(path);
        //    await SaveAsync(token);
        //}

        public void Insert(string path)
        {
            //Remove(path);
            _items.Remove(path);
            //List.Insert(0, path);
            _items.Insert(0, path);

            for (var i = _items.Count - 1; i >= MaxCount; i--)
                _items.RemoveAt(i);
        }

        //private void Remove(string path)
        //{
        //    var t = Math.Min(List.Count, MaxCount);

        //    for (var i = t - 1; i >= 0; i--) {
        //        if (string.Equals(List[i], path))
        //            List.RemoveAt(i);
        //    }
        //}
        public void Remove(string path)
        {
            _items.Remove(path);
        }
    }
}
