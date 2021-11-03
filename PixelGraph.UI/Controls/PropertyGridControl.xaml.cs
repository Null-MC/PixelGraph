using MahApps.Metro.Controls;
using Ookii.Dialogs.Wpf;
using PixelGraph.Common.Extensions;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Models.PropertyGrid;
using System;
using System.IO;
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

        public string ProjectRootPath {
            get => (string)GetValue(ProjectRootPathProperty);
            set => SetValue(ProjectRootPathProperty, value);
        }


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

        private string GetSourcePath(IEditPropertyRow editRow)
        {
            if (editRow.EditValue is string sourceValue) {
                if (File.Exists(sourceValue)) return sourceValue;

                var fullPath = PathEx.Join(ProjectRootPath, sourceValue);
                fullPath = Path.GetFullPath(fullPath);

                if (File.Exists(fullPath)) return fullPath;
            }

            // WARN: Add DefaultDirectory option instead since the control isn't type specific
            //var modelsPath = PathEx.Join(ProjectRootPath, "assets/minecraft/models");

            //if (Directory.Exists(modelsPath))
            //    return modelsPath;

            return null;
        }

        private void OnSelectFileClick(object sender, RoutedEventArgs e)
        {
            var button = sender as UIElement;
            var row = button?.FindParent<DataGridRow>();
            if (row?.DataContext is not IEditTextPropertyRow editRow) return;

            if (string.IsNullOrEmpty(ProjectRootPath)) {
                // TODO: log and alert
                return;
            }

            var dialog = new VistaOpenFileDialog {
                Title = "Select Model File",
                Filter = "JSON File|*.json|All Files|*.*",
                CheckFileExists = true,
            };

            var sourcePath = GetSourcePath(editRow);
            if (sourcePath != null) {
                dialog.InitialDirectory = Path.GetDirectoryName(sourcePath);
                dialog.FileName = sourcePath;
            }

            var window = Window.GetWindow(this);
            if (dialog.ShowDialog(window) != true) return;

            if (PathEx.TryGetRelative(ProjectRootPath, dialog.FileName, out var localPath)) {
                editRow.EditValue = localPath;
            }
            else {
                if (window != null) MessageBox.Show(window, "The selected path must be within the project root!", "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //private void OnSelectColorClick(object sender, RoutedEventArgs e)
        //{
        //    var button = sender as UIElement;
        //    var row = button?.FindParent<DataGridRow>();
        //    if (row?.DataContext is not IEditTextPropertyRow editRow) return;

        //    throw new NotImplementedException();
        //}

        public static readonly DependencyProperty ProjectRootPathProperty = DependencyProperty
            .Register(nameof(ProjectRootPath), typeof(string), typeof(PropertyGridControl));
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
