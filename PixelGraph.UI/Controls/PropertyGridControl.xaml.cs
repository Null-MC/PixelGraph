using PixelGraph.UI.Internal;
using PixelGraph.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace PixelGraph.UI.Controls
{
    public partial class PropertyGridControl
    {
        public PropertyGridControl()
        {
            InitializeComponent();
        }

        private void OnPreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.EditingElement is not ContentPresenter contentPresenter) return;

            contentPresenter.FindChild<TextBox>()?.Focus();

            var comboBox = contentPresenter.FindChild<ComboBox>();
            if (comboBox != null) comboBox.IsDropDownOpen = true;
        }
    }

    public class PropertyGridCellTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextBoxTemplate {get; set;}
        public DataTemplate ComboBoxTemplate {get; set;}


        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch {
                ISelectPropertyRow => ComboBoxTemplate,
                ITextPropertyRow => TextBoxTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }
    }
}
