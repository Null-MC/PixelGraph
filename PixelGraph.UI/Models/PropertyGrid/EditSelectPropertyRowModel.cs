using System;
using System.Collections.Generic;
using System.Linq;

namespace PixelGraph.UI.Models.PropertyGrid
{
    public interface ISelectPropertyRow : IPropertyRow
    {
        string DisplayValue {get;}
        SelectPropertyRowOptions Options {get;}
    }

    public interface IEditSelectPropertyRow<in TProperty> : ISelectPropertyRow, IEditPropertyRow<TProperty> {}

    public class EditSelectPropertyRowModel<TProperty, TValue> : EditPropertyRowModelBase<TProperty, TValue>, IEditSelectPropertyRow<TProperty>
    {
        public SelectPropertyRowOptions Options {get;}
        public string DisplayValue => Options.GetDisplayValue(EditValue);


        public EditSelectPropertyRowModel(string name, string propertyName, SelectPropertyRowOptions options, object defaultValue = null) : base(name, propertyName, defaultValue)
        {
            Options = options;
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
}
