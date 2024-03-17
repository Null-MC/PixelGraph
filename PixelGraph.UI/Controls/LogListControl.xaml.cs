using MahApps.Metro.Controls;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace PixelGraph.UI.Controls;

public partial class LogListControl
{
    private const int LinesPerParagraph = 20;

    private readonly ConcurrentQueue<Message> queue;
    private readonly Lazy<ScrollViewer?> scrollViewerTask;
    private readonly Thickness marginSize;
    private Paragraph? currentParagraph;
    private int paragraphLineCount;


    public LogListControl()
    {
        InitializeComponent();

        queue = new ConcurrentQueue<Message>();
        scrollViewerTask = new Lazy<ScrollViewer?>(GetScrollViewer);
        marginSize = new Thickness(0);
    }

    public void Append(LogLevel level, string text)
    {
        queue.Enqueue(new Message(level, text));

        this.BeginInvoke(() => {
            var any = false;
            while (queue.TryDequeue(out var message)) {
                AppendInternal(message);
                if (!any) any = true;
            }

            if (any) scrollViewerTask.Value?.ScrollToBottom();
        });
    }

    private void AppendInternal(Message message)
    {
        if (currentParagraph == null) {
            currentParagraph = new Paragraph {
                BreakPageBefore = false,
                KeepTogether = true,
                Margin = marginSize,
            };

            Document.Blocks.Add(currentParagraph);
        }

        var run = new Run(message.Text) {
            Foreground = logBrushMap[message.Level],
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
    }

    private ScrollViewer? GetScrollViewer()
    {
        DependencyObject obj = this;

        do {
            if (VisualTreeHelper.GetChildrenCount(obj) <= 0) return null;
            if (obj is not Visual visualObj) return null;
            obj = VisualTreeHelper.GetChild(visualObj, 0);
        }
        while (obj is not ScrollViewer);

        return obj as ScrollViewer;
    }

    private static readonly Dictionary<LogLevel, Brush> logBrushMap = new() {
        [LogLevel.None] = Brushes.LightGray,
        [LogLevel.Debug] = Brushes.LimeGreen,
        [LogLevel.Information] = Brushes.LightSkyBlue,
        [LogLevel.Warning] = Brushes.Yellow,
        [LogLevel.Error] = Brushes.OrangeRed,
        [LogLevel.Critical] = Brushes.Red,
        [LogLevel.Trace] = Brushes.Purple,
    };

    private class Message(LogLevel level, string text)
    {
        public readonly LogLevel Level = level;
        public readonly string Text = text;
    }
}
