using Microsoft.Extensions.DependencyInjection;
using Ookii.Dialogs.Wpf;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.Material;
using PixelGraph.UI.Internal.Projects;
using PixelGraph.UI.Models;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace PixelGraph.UI.Controls
{
    public partial class MaterialPropertiesControl
    {
        private IProjectContextManager projectContextMgr;

        public event EventHandler GenerateNormal;
        public event EventHandler GenerateOcclusion;
        public event EventHandler<MaterialPropertyChangedEventArgs> DataChanged;
        public event EventHandler ModelChanged;

        public MaterialProperties Material {
            //get => (MaterialProperties)GetValue(MaterialProperty);
            set => SetValue(MaterialProperty, value);
        }

        public string SelectedTag {
            //get => (string)GetValue(SelectedTagProperty);
            set => SetValue(SelectedTagProperty, value);
        }


        public MaterialPropertiesControl()
        {
            InitializeComponent();
        }

        public void Initialize(IServiceProvider provider)
        {
            projectContextMgr = provider.GetRequiredService<IProjectContextManager>();
        }

        private void OnSelectFile(object sender, SelectFileEventArgs e)
        {
            var projectContext = projectContextMgr.GetContext();
            if (string.IsNullOrEmpty(projectContext.RootDirectory)) return;

            string sourcePath = null;
            if (e.Value is string sourceValue) {
                if (File.Exists(sourceValue)) {
                    sourcePath = sourceValue;
                }
                else {
                    var fullPath = PathEx.Join(projectContext.RootDirectory, sourceValue);
                    fullPath = Path.GetFullPath(fullPath);

                    if (File.Exists(fullPath))
                        sourcePath = fullPath;
                }
            }

            if (sourcePath == null) {
                // TODO: set to material path
            }

            // WARN: Add DefaultDirectory option instead since the control isn't type specific
            //var modelsPath = PathEx.Join(ProjectRootPath, "assets/minecraft/models");

            //if (Directory.Exists(modelsPath))
            //    return modelsPath;

            var dialog = new VistaOpenFileDialog {
                Title = "Select Model File",
                Filter = "JSON File|*.json|All Files|*.*",
                CheckFileExists = true,
            };

            if (sourcePath != null) {
                dialog.InitialDirectory = Path.GetDirectoryName(sourcePath);
                dialog.FileName = sourcePath;
            }

            var window = Window.GetWindow(this);
            if (dialog.ShowDialog(window) != true) return;

            if (PathEx.TryGetRelative(projectContext.RootDirectory, dialog.FileName, out var localPath)) {
                e.Value = localPath;
                e.Success = true;
            }
            else {
                if (window != null) MessageBox.Show(window, "The selected path must be within the project root!", "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        protected virtual void OnDataChanged(object sender, MaterialPropertyChangedEventArgs e)
        {
            DataChanged?.Invoke(this, e);
        }

        protected virtual void OnModelChanged()
        {
            ModelChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnGenerateNormalClick(object sender, RoutedEventArgs e)
        {
            GenerateNormal?.Invoke(this, EventArgs.Empty);
        }

        private void OnGenerateOcclusionClick(object sender, RoutedEventArgs e)
        {
            GenerateOcclusion?.Invoke(this, EventArgs.Empty);
        }

        private void OnIorToFoConvertButtonClick(object sender, RoutedEventArgs e)
        {
            Model.ConvertIorToF0();
        }

        private void OnF0ConverterTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                IorToF0ConvertButton.Focus();
                Model.ConvertIorToF0();
            }
        }

        private void OnPropertyGridModelChanged(object sender, EventArgs e)
        {
            OnModelChanged();
        }

        private static void OnMaterialPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not MaterialPropertiesControl control) return;

            control.Model.Material = e.NewValue as MaterialProperties;
        }

        private static void OnSelectedTagPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not MaterialPropertiesControl control) return;

            control.Model.SelectedTag = e.NewValue as string;
        }

        public static readonly DependencyProperty MaterialProperty = DependencyProperty
            .Register(nameof(Material), typeof(MaterialProperties), typeof(MaterialPropertiesControl), new PropertyMetadata(OnMaterialPropertyChanged));

        public static readonly DependencyProperty SelectedTagProperty = DependencyProperty
            .Register(nameof(SelectedTag), typeof(string), typeof(MaterialPropertiesControl), new PropertyMetadata(OnSelectedTagPropertyChanged));
    }
}
