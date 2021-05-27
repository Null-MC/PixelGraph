using MahApps.Metro.Controls;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace PixelGraph.UI.Controls
{
    public partial class LogListControl
    {
        private const int LinesPerParagraph = 20;

        private readonly Lazy<ScrollViewer> scrollViewerTask;
        private readonly FontFamily fontFamily;
        private readonly Thickness marginSize;
        private Paragraph currentParagraph;
        private int paragraphLineCount;


        public LogListControl()
        {
            InitializeComponent();

            scrollViewerTask = new Lazy<ScrollViewer>(GetScrollViewer);
            fontFamily = new FontFamily("Consolas");
            marginSize = new Thickness(0);
        }

        public void Append(LogLevel level, string text)
        {
            this.BeginInvoke(() => AppendInternal(level, text));
        }

        private void AppendInternal(LogLevel level, string text)
        {
            if (currentParagraph == null) {
                currentParagraph = new Paragraph {
                    BreakPageBefore = false,
                    KeepTogether = true,
                    //FontFamily = fontFamily,
                    Margin = marginSize,
                    //FontSize = 12,
                };

                Document.Blocks.Add(currentParagraph);
            }

            var run = new Run(text) {
                Foreground = logBrushMap[level],
            };

            currentParagraph.Inlines.Add(run);
            paragraphLineCount++;

            if (paragraphLineCount > LinesPerParagraph) {
                currentParagraph = null;
                paragraphLineCount = 0;
            }
            else {
                currentParagraph.Inlines.Add(new LineBreak());
            }

            scrollViewerTask.Value.ScrollToBottom();
        }

        private ScrollViewer GetScrollViewer()
        {
            //var listbox = (ListBox)VisualTreeHelper.GetChild(this, 0);
            //var border = (Border)VisualTreeHelper.GetChild(listbox, 0);
            //return (ScrollViewer)VisualTreeHelper.GetChild(border, 0);

            DependencyObject obj = this;

            do {
                if (VisualTreeHelper.GetChildrenCount(obj) > 0)
                    obj = VisualTreeHelper.GetChild(obj as Visual, 0);
                else
                    return null;
            }
            while (!(obj is ScrollViewer));

            return obj as ScrollViewer;
        }

        private static readonly Dictionary<LogLevel, Brush> logBrushMap = new Dictionary<LogLevel, Brush> {
            [LogLevel.None] = Brushes.LightGray,
            [LogLevel.Debug] = Brushes.LimeGreen,
            [LogLevel.Information] = Brushes.LightSkyBlue,
            [LogLevel.Warning] = Brushes.Yellow,
            [LogLevel.Error] = Brushes.OrangeRed,
            [LogLevel.Critical] = Brushes.Red,
            [LogLevel.Trace] = Brushes.Purple,
        };
    }
}
