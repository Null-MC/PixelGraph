using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.ViewModels;
using System;
using System.Windows;

namespace PixelGraph.UI.Windows
{
    public partial class PublishWindow : Window
    {
        public PublishWindow(IServiceProvider provider)
        {
            DataContext = provider.GetRequiredService<PublishWindowVM>();

            InitializeComponent();
        }
    }
}
