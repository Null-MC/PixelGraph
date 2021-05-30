using System.Windows;
using System.Windows.Media;

namespace PixelGraph.UI.Internal
{
    internal static class VisualTreeHelperEx
    {
        public static T FindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            while (true) {
                var parentObject = VisualTreeHelper.GetParent(child);
                if (parentObject == null) return null;

                if (parentObject is T parent) return parent;
                child = parentObject;
            }
        }

        public static T FindChild<T>(this DependencyObject parent) where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(parent);

            for (var i = 0; i < count; i++) {
                var childObject = VisualTreeHelper.GetChild(parent, i);
                if (childObject is T child) return child;
            }

            return null;
        }
    }
}
