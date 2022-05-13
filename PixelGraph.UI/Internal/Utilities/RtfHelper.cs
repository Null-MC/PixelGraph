using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;

namespace PixelGraph.UI.Internal.Utilities
{
    public static class RtfHelper
    {
        public static FlowDocument LoadDocument(string path)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(path);
            if (stream == null) throw new ApplicationException("Failed to load document!");

            var document = new FlowDocument();

            document.BeginInit();
            document.TextAlignment = TextAlignment.Justify;
            var range = new TextRange(document.ContentStart, document.ContentEnd);
            range.Load(stream, DataFormats.Rtf);
            document.EndInit();
            
            return document;
        }

        public static IEnumerable<DependencyObject> GetVisuals(DependencyObject root)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>()) {
                yield return child;
                foreach (var descendants in GetVisuals(child))
                    yield return descendants;
            }
        }
    }
}
