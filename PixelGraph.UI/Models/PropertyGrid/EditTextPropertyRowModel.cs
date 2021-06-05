namespace PixelGraph.UI.Models.PropertyGrid
{
    public interface ITextPropertyRow : IPropertyRow {}

    public interface IEditTextPropertyRow<in TProperty> : ITextPropertyRow, IEditPropertyRow<TProperty> {}

    public class EditTextPropertyRowModel<TProperty, TValue> : EditPropertyRowModelBase<TProperty, TValue>, IEditTextPropertyRow<TProperty>
    {
        public EditTextPropertyRowModel(string name, string propertyName, object defaultValue = null) : base(name, propertyName, defaultValue) {}
    }
}
