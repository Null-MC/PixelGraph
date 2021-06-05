using PixelGraph.UI.ViewModels;

namespace PixelGraph.UI.Models.PropertyGrid
{
    public interface IBoolPropertyRow : IPropertyRow {}

    public interface IEditBoolPropertyRow<in TProperty> : IBoolPropertyRow, IEditPropertyRow<TProperty> {}

    public class EditBoolPropertyRowModel<TProperty, TValue> : EditPropertyRowModelBase<TProperty, TValue>, IEditBoolPropertyRow<TProperty>
    {
        public EditBoolPropertyRowModel(string name, string propertyName, object defaultValue = null) : base(name, propertyName, defaultValue) {}
    }
}
