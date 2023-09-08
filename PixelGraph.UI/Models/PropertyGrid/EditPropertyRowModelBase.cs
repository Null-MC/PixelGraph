﻿using PixelGraph.Common.Extensions;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.ViewModels;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Media;

namespace PixelGraph.UI.Models.PropertyGrid;

public interface IPropertyRow : INotifyPropertyChanged
{
    bool Enabled {get; set;}
    object ActualValue {get;}
}

public interface IEditPropertyRow : IPropertyRow
{
    string Name {get;}
    object EditValue {get; set;}
    object EditDisplayValue {get;}
    object DefaultValue {get; set;}
}

public interface IEditPropertyRow<in TProperty> : IEditPropertyRow
{
    void SetData(TProperty data);
    void Invalidate();
}

public abstract class EditPropertyRowModelBase<TProperty, TValue> : ModelBase, IEditPropertyRow<TProperty>
{
    private readonly PropertyInfo info;
    private readonly string propertyName;
    private object _defaultValue;
    private TProperty _data;

    public event EventHandler<PropertyValueChangedEventArgs> ValueChanged;

    public string Name {get;}
    public bool Enabled {get; set;}

    public object EditDisplayValue => GetDisplayValue();

    public object ActualValue {
        get {
            if (_data == null) return null;
            return info.GetValue(_data);
        }
    }

    public object EditValue {
        get => ActualValue ?? _defaultValue;
        set {
            if (_data == null) return;
            info.SetValue(_data, FormatValue<TValue>(value));
            OnValueChanged();
            OnPropertyChanged();
            OnPropertyChanged(nameof(ActualValue));
            OnPropertyChanged(nameof(EditDisplayValue));
        }
    }

    public object DefaultValue {
        get => _defaultValue;
        set {
            if (_data == null) return;
            _defaultValue = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(EditValue));
            OnPropertyChanged(nameof(EditDisplayValue));
        }
    }


    protected EditPropertyRowModelBase(string name, string propertyName, object defaultValue = null)
    {
        this.propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        Name = name ?? throw new ArgumentNullException(nameof(name));

        info = typeof(TProperty).GetProperty(propertyName);
        if (info == null) throw new ApplicationException($"Property '{typeof(TProperty).Name}.{propertyName}' not found!");

        _defaultValue = defaultValue;
        Enabled = true;
    }

    protected virtual T FormatValue<T>(object value)
    {
        if (value is string stringValue) {
            if (TryParseDivision(stringValue, out var decimalValue))
                return decimalValue.To<T>();
        }

        if (value is Color colorValue && typeof(T) == typeof(string))
            return ColorHelper.ToHexRGB(colorValue).To<T>();

        return value.To<T>();
    }

    public void SetData(TProperty data)
    {
        _data = data;
        Invalidate();
    }

    public virtual void Invalidate()
    {
        OnPropertyChanged(nameof(ActualValue));
        OnPropertyChanged(nameof(EditValue));
        OnPropertyChanged(nameof(EditDisplayValue));
    }

    protected virtual object GetDisplayValue() => EditValue;

    protected virtual void OnValueChanged()
    {
        var e = new PropertyValueChangedEventArgs {
            PropertyName = propertyName,
        };

        ValueChanged?.Invoke(this, e);
    }

    private static bool TryParseDivision(string text, out decimal value)
    {
        var i = text.IndexOf('/');
        if (i < 0) {
            value = 0m;
            return false;
        }

        var numeratorString = text[..i].Trim();
        if (!decimal.TryParse(numeratorString, out var numerator)) {
            value = 0m;
            return false;
        }

        var denominatorString = text[(i + 1)..].Trim();
        if (!decimal.TryParse(denominatorString, out var denominator)) {
            value = 0m;
            return false;
        }

        value = numerator / denominator;
        return true;
    }
}