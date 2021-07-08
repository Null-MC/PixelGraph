using PixelGraph.Common.Extensions;
using PixelGraph.UI.Internal;
using PixelGraph.UI.ViewModels;
using System;
using System.ComponentModel;
using System.Reflection;

namespace PixelGraph.UI.Models.PropertyGrid
{
    public interface IPropertyRow : INotifyPropertyChanged
    {
        string Name {get;}
        object ActualValue {get;}
        object EditValue {get; set;}
    }

    public interface IEditPropertyRow<in TProperty> : IPropertyRow
    {
        void SetData(TProperty data);
        void Invalidate();
    }

    public abstract class EditPropertyRowModelBase<TProperty, TValue> : ModelBase, IEditPropertyRow<TProperty>
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


        protected EditPropertyRowModelBase(string name, string propertyName, object defaultValue = null)
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
}
