using PixelGraph.UI.Internal;
using PixelGraph.UI.Models.PropertyGrid;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PixelGraph.UI.Controls
{
    public partial class PropertyGridControl
    {
        private bool isEditing;

        public event EventHandler<PropertyGridChangedEventArgs> PropertyChanged;


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
            
            foreach (var row in SelectedItems.OfType<IEditPropertyRow>())
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

            if (e.EditAction != DataGridEditAction.Commit) return;
            if (e.Row.Item is not IEditPropertyRow editRow) return;

            OnPropertyChanged(editRow.Name);
        }

        private void OnCurrentCellChanged(object sender, EventArgs e)
        {
            // WARN: WTF is this?!
            //if (CurrentCell.Item is not IEditPropertyRow editRow) return;

            //OnPropertyChanged(editRow.Name);
        }

        private void OnPropertyChanged(string propertyName)
        {
            var e = new PropertyGridChangedEventArgs {
                PropertyName = propertyName,
            };

            PropertyChanged?.Invoke(this, e);
        }
    }

    public class PropertyGridCellTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextBoxTemplate {get; set;}
        public DataTemplate CheckBoxTemplate {get; set;}
        public DataTemplate ComboBoxTemplate {get; set;}
        public DataTemplate SeparatorTemplate {get; set;}


        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch {
                ITextPropertyRow => TextBoxTemplate,
                IBoolPropertyRow => CheckBoxTemplate,
                ISelectPropertyRow => ComboBoxTemplate,
                ISeparatorPropertyRow => SeparatorTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }
    }

    public class PropertyGridChangedEventArgs : EventArgs
    {
        public string PropertyName {get; set;}
    }
}
