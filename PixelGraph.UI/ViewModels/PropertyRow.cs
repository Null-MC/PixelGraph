using PixelGraph.UI.Internal.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace PixelGraph.UI.ViewModels
{
    public interface IPropertyRow : INotifyPropertyChanged
    {
        string Name {get;}
        object ActualValue {get;}
        object EditValue {get; set;}
    }

    public interface ITextPropertyRow : IPropertyRow {}

    public interface IBoolPropertyRow : IPropertyRow {}

    public interface ISelectPropertyRow : IPropertyRow
    {
        string DisplayValue {get;}
        SelectPropertyRowOptions Options {get;}
    }

    public interface IEditPropertyRow<in TProperty> : IPropertyRow
    {
        void SetData(TProperty data);
        void Invalidate();
    }

    public interface IEditTextPropertyRow<in TProperty> : ITextPropertyRow, IEditPropertyRow<TProperty> {}

    public interface IEditBoolPropertyRow<in TProperty> : IBoolPropertyRow, IEditPropertyRow<TProperty> {}

    public interface IEditSelectPropertyRow<in TProperty> : ISelectPropertyRow, IEditPropertyRow<TProperty> {}

    public class EditTextPropertyRow<TProperty, TValue> : EditPropertyRowBase<TProperty, TValue>, IEditTextPropertyRow<TProperty>
    {
        public EditTextPropertyRow(string name, string propertyName, object defaultValue = null) : base(name, propertyName, defaultValue) {}
    }

    public class EditBoolPropertyRow<TProperty, TValue> : EditPropertyRowBase<TProperty, TValue>, IEditBoolPropertyRow<TProperty>
    {
        public EditBoolPropertyRow(string name, string propertyName, object defaultValue = null) : base(name, propertyName, defaultValue) {}
    }

    public class EditSelectPropertyRow<TProperty, TValue> : EditPropertyRowBase<TProperty, TValue>, IEditSelectPropertyRow<TProperty>
    {
        public SelectPropertyRowOptions Options {get;}
        public string DisplayValue => Options.GetDisplayValue(EditValue);


        public EditSelectPropertyRow(string name, string propertyName, SelectPropertyRowOptions options, object defaultValue = null) : base(name, propertyName, defaultValue)
        {
            Options = options;
        }
    }

    public abstract class EditPropertyRowBase<TProperty, TValue> : ViewModelBase, IEditPropertyRow<TProperty>
    {
        private readonly PropertyInfo info;
        private readonly string propertyName;
        private readonly object _defaultValue;
        private TProperty _data;

        public event EventHandler<PropertyValueChangedEventArgs> ValueChanged;

        public string Name {get;}

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
                info.SetValue(_data, value.To<TValue>());
                OnValueChanged();
                OnPropertyChanged();
                OnPropertyChanged(nameof(ActualValue));
            }
        }


        protected EditPropertyRowBase(string name, string propertyName, object defaultValue = null)
        {
            this.propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            Name = name ?? throw new ArgumentNullException(nameof(name));

            info = typeof(TProperty).GetProperty(propertyName);
            if (info == null) throw new ApplicationException($"Property '{typeof(TProperty).Name}.{propertyName}' not found!");

            _defaultValue = defaultValue;
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
        }

        private void OnValueChanged()
        {
            var e = new PropertyValueChangedEventArgs {
                PropertyName = propertyName,
            };

            ValueChanged?.Invoke(this, e);
        }
    }

    public class SelectPropertyRowOptions
    {
        public IEnumerable<object> Items {get; set;}
        public string DisplayMemberPath {get; set;}
        public string SelectedValuePath {get; set;}


        public SelectPropertyRowOptions(IEnumerable<object> items, string display, string value)
        {
            Items = items;
            DisplayMemberPath = display;
            SelectedValuePath = value;
        }

        public string GetDisplayValue(object value)
        {
            var itemType = Items.FirstOrDefault()?.GetType();
            if (itemType == null) return null;

            var displayProperty = itemType.GetProperty(DisplayMemberPath);
            var valueProperty = itemType.GetProperty(SelectedValuePath);
            if (displayProperty == null || valueProperty == null) return null;

            var item = Items.FirstOrDefault(i => {
                var itemValue = valueProperty.GetValue(i);

                if (itemValue is string v1 && value is string v2)
                    return string.Equals(v1, v2, StringComparison.InvariantCultureIgnoreCase);

                return itemValue == value;
            });

            if (item == null) return null;

            return (string)displayProperty.GetValue(item);
        }
    }

    public class PropertyValueChangedEventArgs : EventArgs
    {
        public string PropertyName {get; set;}
    }
}
