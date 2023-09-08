namespace PixelGraph.UI.Models.PropertyGrid;

public interface ITextPropertyRow : IPropertyRow
{
    bool IsNumeric {get; set;}
    bool IsNumericRange {get; set;}
    bool IsFileSelect {get; set;}
    bool IsColorSelect {get; set;}

    decimal RangeMin {get; set;}
    decimal RangeMax {get; set;}
}

public interface IEditTextPropertyRow : ITextPropertyRow, IEditPropertyRow {}

public interface IEditTextPropertyRow<in TProperty> : IEditTextPropertyRow, IEditPropertyRow<TProperty> {}

public class EditTextPropertyRowModel<TProperty, TValue> : EditPropertyRowModelBase<TProperty, TValue>, IEditTextPropertyRow<TProperty>
{
    public bool IsNumeric {get; set;}
    public bool IsNumericRange {get; set;}
    public bool IsFileSelect {get; set;}
    public bool IsColorSelect {get; set;}

    public decimal RangeMin {get; set;}
    public decimal RangeMax {get; set;}
        
    public EditTextPropertyRowModel(string name, string propertyName, object defaultValue = null) : base(name, propertyName, defaultValue) {}
}