using PixelGraph.UI.Models.PropertyGrid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PixelGraph.UI.ViewModels;

public abstract class PropertyCollectionBase<TSource> : List<IPropertyRow>, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected TSource Data {get; set;}


    protected IEditTextPropertyRow<TSource> AddValue<TValue>(string displayName, string propertyName, TValue defaultValue = default)
    {
        var row = new EditTextPropertyRowModel<TSource, TValue>(displayName, propertyName, defaultValue);
        row.IsNumeric = true;
        row.ValueChanged += OnRowValueChanged;
        Add(row);

        return row;
    }

    protected IEditTextPropertyRow<TSource> AddValue<TValue>(string displayName, string propertyName, decimal min, decimal max, TValue defaultValue = default)
    {
        var row = new EditTextPropertyRowModel<TSource, TValue>(displayName, propertyName, defaultValue);
        row.IsNumericRange = true;
        row.RangeMin = min;
        row.RangeMax = max;
        row.ValueChanged += OnRowValueChanged;
        Add(row);

        return row;
    }

    protected IEditTextPropertyRow<TSource> AddText<TValue>(string displayName, string propertyName, TValue defaultValue = default)
    {
        var row = new EditTextPropertyRowModel<TSource, TValue>(displayName, propertyName, defaultValue);
        row.ValueChanged += OnRowValueChanged;
        Add(row);

        return row;
    }

    protected IEditTextPropertyRow<TSource> AddTextFile<TValue>(string displayName, string propertyName, TValue defaultValue = default)
    {
        var row = new EditTextPropertyRowModel<TSource, TValue>(displayName, propertyName, defaultValue) {
            IsFileSelect = true,
        };

        row.ValueChanged += OnRowValueChanged;
        Add(row);

        return row;
    }

    protected IEditTextPropertyRow<TSource> AddTextColor<TValue>(string displayName, string propertyName, TValue defaultValue = default)
    {
        var row = new EditTextPropertyRowModel<TSource, TValue>(displayName, propertyName, defaultValue) {
            IsColorSelect = true,
        };

        row.ValueChanged += OnRowValueChanged;
        Add(row);

        return row;
    }

    protected IEditBoolPropertyRow<TSource> AddBool<TValue>(string displayName, string propertyName, TValue defaultValue = default)
    {
        var row = new EditBoolPropertyRowModel<TSource, TValue>(displayName, propertyName, defaultValue);
        row.ValueChanged += OnRowValueChanged;
        Add(row);

        return row;
    }

    protected IEditSelectPropertyRow<TSource> AddSelect<TValue>(string displayName, string propertyName, SelectPropertyRowOptions options, TValue defaultValue = default)
    {
        var row = new EditSelectPropertyRowModel<TSource, TValue>(displayName, propertyName, options, defaultValue);
        row.ValueChanged += OnRowValueChanged;
        Add(row);

        return row;
    }

    protected IEditSelectPropertyRow<TSource> AddTextSelect<TValue>(string displayName, string propertyName, SelectPropertyRowOptions options, TValue defaultValue = default)
    {
        var row = new EditSelectPropertyRowModel<TSource, TValue>(displayName, propertyName, options, defaultValue) {
            CanEditText = true,
        };

        row.ValueChanged += OnRowValueChanged;
        Add(row);

        return row;
    }

    //protected EditSelectPropertyRowModel<TSource, TValue> AddTextSelect<TValue>(string displayName, string propertyName, SelectPropertyRowOptions options, Func<TValue> defaultFunc)
    //{
    //    var row = new EditSelectPropertyRowModel<TSource, TValue>(displayName, propertyName, options, defaultFunc) {
    //        CanEditText = true
    //    };

    //    row.ValueChanged += OnRowValueChanged;
    //    Add(row);

    //    return row;
    //}

    protected void AddSeparator()
    {
        var row = new SeparatorPropertyRowModel();
        Add(row);
    }

    protected virtual void OnRowValueChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public virtual void SetData(TSource data)
    {
        Data = data;

        foreach (var row in GetRows())
            row.SetData(data);
    }

    public void Invalidate()
    {
        foreach (var row in GetRows())
            row.Invalidate();
    }

    private IEnumerable<IEditPropertyRow<TSource>> GetRows()
    {
        return this.OfType<IEditPropertyRow<TSource>>();
    }

    private void OnRowValueChanged(object sender, PropertyValueChangedEventArgs e)
    {
        OnRowValueChanged(e.PropertyName);
    }
}

public class PropertyValueChangedEventArgs : EventArgs
{
    public string PropertyName {get; set;}
}