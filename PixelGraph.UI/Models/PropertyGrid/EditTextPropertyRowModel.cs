namespace PixelGraph.UI.Models.PropertyGrid
{
    public interface ITextPropertyRow : IPropertyRow
    {
        bool IsFileSelect {get; set;}
    }

    public interface IEditTextPropertyRow : ITextPropertyRow, IEditPropertyRow {}

    public interface IEditTextPropertyRow<in TProperty> : IEditTextPropertyRow, IEditPropertyRow<TProperty> {}

    public class EditTextPropertyRowModel<TProperty, TValue> : EditPropertyRowModelBase<TProperty, TValue>, IEditTextPropertyRow<TProperty>
    {
        public bool IsFileSelect {get; set;}
        
        public EditTextPropertyRowModel(string name, string propertyName, object defaultValue = null) : base(name, propertyName, defaultValue) {}
    }
}
