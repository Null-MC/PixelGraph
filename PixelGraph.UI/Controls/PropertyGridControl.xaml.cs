using PixelGraph.UI.Internal;
using PixelGraph.UI.Models.PropertyGrid;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PixelGraph.UI.Controls
{
    public partial class PropertyGridControl
    {
        private bool isEditing;


        public PropertyGridControl()
        {
            InitializeComponent();
        }

        private void OnPreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.EditingElement is not ContentPresenter contentPresenter) return;

            var textBox = contentPresenter.FindChild<TextBox>();
            if (textBox != null) {
                textBox.Focus();
                textBox.SelectAll();
                return;
            }

            var comboBox = contentPresenter.FindChild<ComboBox>();
            if (comboBox != null) comboBox.IsDropDownOpen = true;

            var checkBox = contentPresenter.FindChild<CheckBox>();
            if (checkBox != null) checkBox.IsChecked = !(checkBox.IsChecked ?? false);
        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (isEditing || e.Key != Key.Delete) return;
            
            foreach (var row in SelectedItems.OfType<IPropertyRow>())
                row.EditValue = null;

            e.Handled = true;
        }

        private void OnBeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Column != null) isEditing = true;
        }

        private void OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            isEditing = false;
        }
    }

    public class PropertyGridCellTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextBoxTemplate {get; set;}
        public DataTemplate CheckBoxTemplate {get; set;}
        public DataTemplate ComboBoxTemplate {get; set;}


        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch {
                ITextPropertyRow => TextBoxTemplate,
                IBoolPropertyRow => CheckBoxTemplate,
                ISelectPropertyRow => ComboBoxTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }
    }
}
