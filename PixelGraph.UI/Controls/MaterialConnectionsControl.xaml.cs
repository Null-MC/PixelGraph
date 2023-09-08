using PixelGraph.Common.Material;
using System;
using System.Windows;

namespace PixelGraph.UI.Controls;

public partial class MaterialConnectionsControl
{
    public event EventHandler DataChanged;

    public MaterialProperties Material {
        set => SetValue(MaterialProperty, value);
    }


    public MaterialConnectionsControl()
    {
        InitializeComponent();
    }

    private void OnDataChanged(object sender, EventArgs e)
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnFilterPropertyChanged(object sender, PropertyGridChangedEventArgs e)
    {
        //var row = Model.SelectedFilter;
        //if (row == null) return;

        //row.NotifyPropertyChanged(e.PropertyName);

        //switch (e.PropertyName) {
        //    case nameof(MaterialFilter.Left):
        //    case nameof(MaterialFilter.Top):
        //    case nameof(MaterialFilter.Width):
        //    case nameof(MaterialFilter.Height):
        //        // TODO: update outline
        //        InvalidateSelectedFilterBounds();
        //        break;
        //}
    }

    private static void OnMaterialPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not MaterialConnectionsControl control) return;
        control.Model.Material = e.NewValue as MaterialProperties;
    }

    public static readonly DependencyProperty MaterialProperty = DependencyProperty
        .Register(nameof(Material), typeof(MaterialProperties), typeof(MaterialConnectionsControl), new PropertyMetadata(OnMaterialPropertyChanged));
}