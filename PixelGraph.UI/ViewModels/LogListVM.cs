using Microsoft.Extensions.Logging;
using PixelGraph.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace PixelGraph.UI.ViewModels
{
    public class LogListVM : ViewModelBase
    {
        public event EventHandler Appended;

        public ObservableCollection<LogMessageItem> Items {get;}


        public LogListVM()
        {
            Items = new ObservableCollection<LogMessageItem>();
        }

        public void Append(LogLevel level, string message)
        {
            var item = new LogMessageItem {
                Message = message,
                Color = logBrushMap.Get(level, Brushes.LightSlateGray),
            };

            Items.Add(item);

            Appended?.Invoke(this, EventArgs.Empty);
        }

        public void BeginAppend(LogLevel level, string message)
        {
            Application.Current.Dispatcher.BeginInvoke(() => Append(level, message));
        }

        private static readonly Dictionary<LogLevel, Brush> logBrushMap = new Dictionary<LogLevel, Brush> {
            [LogLevel.Debug] = Brushes.LimeGreen,
            [LogLevel.Information] = Brushes.LightSkyBlue,
            [LogLevel.Warning] = Brushes.Yellow,
            [LogLevel.Error] = Brushes.OrangeRed,
            [LogLevel.Critical] = Brushes.Red,
            [LogLevel.Trace] = Brushes.Purple,
        };
    }

    public class LogListDesignVM : LogListVM
    {
        public LogListDesignVM()
        {
            Append(LogLevel.Debug, "DEBUG");
            Append(LogLevel.Information, "INFO");
            Append(LogLevel.Warning, "WARNING");
            Append(LogLevel.Critical, "CRITICAL");
        }
    }
}
