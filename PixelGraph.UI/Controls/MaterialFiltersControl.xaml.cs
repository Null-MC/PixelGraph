﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.Common.Material;
using PixelGraph.UI.Internal.IO.Models;
using PixelGraph.UI.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PixelGraph.UI.Controls;

public partial class MaterialFiltersControl
{
    public event EventHandler? DataChanged;

    private IServiceProvider? provider;
    private ILogger<MaterialFiltersControl>? logger;

    public ITexturePreviewModel TexturePreviewModel {
        get => (ITexturePreviewModel)GetValue(TexturePreviewModelProperty);
        set => SetValue(TexturePreviewModelProperty, value);
    }

    public MaterialProperties Material {
        //get => (MaterialProperties)GetValue(MaterialProperty);
        set => SetValue(MaterialProperty, value);
    }

    public string SelectedTag {
        set => SetValue(SelectedTagProperty, value);
    }


    public MaterialFiltersControl()
    {
        InitializeComponent();
    }

    public void Initialize(IServiceProvider _provider)
    {
        provider = _provider ?? throw new ArgumentNullException(nameof(_provider));

        logger = provider.GetRequiredService<ILogger<MaterialFiltersControl>>();
    }

    protected void OnDataChanged()
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    private void InvalidateSelectedFilterBounds()
    {
        if (Model.SelectedFilter != null) {
            Model.SelectedFilter.Filter.GetRectangle(out var bounds);
            TexturePreviewModel.SetOutlineBounds(bounds);
            return;
        }

        TexturePreviewModel.SetOutlineBounds(null);
    }

    private void OnModelDataChanged(object? sender, EventArgs e)
    {
        OnDataChanged();
    }

    private void OnSelectionChanged(object? sender, EventArgs e)
    {
        InvalidateSelectedFilterBounds();
    }

    private void OnFilterAddButtonClick(object? sender, RoutedEventArgs e)
    {
        Model.AddNewFilter();
    }

    private void OnFilterDeleteButtonClick(object? sender, RoutedEventArgs e)
    {
        Model.DeleteSelectedFilter();
    }

    private void OnFilterImportFromModelButtonClick(object? sender, RoutedEventArgs e)
    {
        var material = Model.Material;
        if (material == null) return;

        ArgumentNullException.ThrowIfNull(provider);

        var window = Window.GetWindow(this)
            ?? throw new ApplicationException("Unable to locate parent window handle!");

        material.Filters ??= [];

        if (material.Filters.Count > 0) {
            var confirm = MessageBox.Show(window, "Would you like to clear the existing filters before importing? This operation cannot be undone!", "Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (confirm == MessageBoxResult.Cancel) return;
                
            if (confirm == MessageBoxResult.Yes)
                material.Filters.Clear();
        }

        try {
            var loader = provider.GetRequiredService<ModelLoader>();
            Model.ImportFiltersFromModel(loader);
            Model.UpdateFilterList();
        }
        catch (Exception error) {
            logger?.LogError(error, "Failed to import UV mappings from entity model!");
            MessageBox.Show(window, "Failed to import UV mappings from entity model!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnFilterPropertyChanged(object? sender, PropertyGridChangedEventArgs e)
    {
        var row = Model.SelectedFilter;
        if (row == null || e.PropertyName == null) return;

        row.NotifyPropertyChanged(e.PropertyName);

        switch (e.PropertyName) {
            case nameof(MaterialFilter.Left):
            case nameof(MaterialFilter.Top):
            case nameof(MaterialFilter.Width):
            case nameof(MaterialFilter.Height):
                // TODO: update outline
                InvalidateSelectedFilterBounds();
                break;
        }
    }

    private void OnFilterListMouseDown(object? sender, MouseButtonEventArgs e)
    {
        var r = VisualTreeHelper.HitTest(this, e.GetPosition(this));

        if (r.VisualHit.GetType() != typeof(ListBoxItem))
            FilterList.UnselectAll();
    }

    private void OnContextMenuDuplicateClick(object? sender, RoutedEventArgs e)
    {
        var selected = Model.SelectedFilter?.Filter.Clone();
        if (selected == null) return;

        Model.AddNewFilter(selected);
    }

    private void OnContextMenuDeleteClick(object? sender, RoutedEventArgs e)
    {
        Model.DeleteSelectedFilter();
    }

    private static void OnTexturePreviewModelPropertyChanged(DependencyObject? sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not MaterialFiltersControl control) return;
        control.Model.TexturePreviewData = e.NewValue as ITexturePreviewModel;
    }

    private static void OnMaterialPropertyChanged(DependencyObject? sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not MaterialFiltersControl control) return;
        control.Model.Material = e.NewValue as MaterialProperties;
    }

    private static void OnSelectedTagPropertyChanged(DependencyObject? sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not MaterialFiltersControl control) return;
        if (control.Model == null) return;

        control.Model.SelectedTag = e.NewValue as string;
    }

    public static readonly DependencyProperty TexturePreviewModelProperty = DependencyProperty
        .Register(nameof(TexturePreviewModel), typeof(ITexturePreviewModel), typeof(MaterialFiltersControl), new PropertyMetadata(OnTexturePreviewModelPropertyChanged));

    public static readonly DependencyProperty MaterialProperty = DependencyProperty
        .Register(nameof(Material), typeof(MaterialProperties), typeof(MaterialFiltersControl), new PropertyMetadata(OnMaterialPropertyChanged));

    public static readonly DependencyProperty SelectedTagProperty = DependencyProperty
        .Register(nameof(SelectedTag), typeof(string), typeof(MaterialFiltersControl), new PropertyMetadata(OnSelectedTagPropertyChanged));
}