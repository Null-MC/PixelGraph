using PixelGraph.UI.Internal;
using PixelGraph.UI.Models.PropertyGrid;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PixelGraph.UI.Controls
{
    public class SelectFileEventArgs : EventArgs
    {
        public object Value {get; set;}
        public bool Success {get; set;}
    }

    public partial class PropertyGridControl
    {
        private bool isEditing;

        public event EventHandler<PropertyGridChangedEventArgs> PropertyChanged;
        public event EventHandler<SelectFileEventArgs> SelectFile;


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

        private void OnSelectFileClick(object sender, RoutedEventArgs e)
        {
            var button = sender as UIElement;
            var row = button?.FindParent<DataGridRow>();
            if (row?.DataContext is not IEditTextPropertyRow editRow) return;

            if (OnSelectFile(editRow.EditValue, out var newValue))
                editRow.EditValue = newValue;
        }

        private bool OnSelectFile(in object value, out object result)
        {
            var e = new SelectFileEventArgs {
                Value = value,
            };

            SelectFile?.Invoke(this, e);

            result = e.Value;
            return e.Success;
        }
    }

    public class PropertyGridRowStyleSelector : StyleSelector
    {
        public Style DefaultStyle {get; set;}
        public Style SeparatorStyle {get; set;}


        public override Style SelectStyle(object item, DependencyObject container)
        {
            return item switch {
                ISeparatorPropertyRow => SeparatorStyle,
                _ => DefaultStyle,
            };
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

    public class PropertyGridEditCellTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextBoxTemplate {get; set;}
        public DataTemplate CheckBoxTemplate {get; set;}
        public DataTemplate ComboBoxTemplate {get; set;}
        public DataTemplate SeparatorTemplate {get; set;}


        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch {
                IEditTextPropertyRow => TextBoxTemplate,
                IEditBoolPropertyRow => CheckBoxTemplate,
                IEditSelectPropertyRow => ComboBoxTemplate,
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
