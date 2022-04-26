using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Utilities;
using PixelGraph.UI.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace PixelGraph.UI.Windows
{
    public partial class PackProfilesWindow
    {
        private readonly IProjectContextManager projectContextMgr;


        public PackProfilesWindow(IServiceProvider provider)
        {
            InitializeComponent();

            var themeHelper = provider.GetRequiredService<IThemeHelper>();
            themeHelper.ApplyCurrent(this);

            projectContextMgr = provider.GetRequiredService<IProjectContextManager>();
            var projectContext = projectContextMgr.GetContext();

            foreach (var profile in projectContext.Project.Profiles)
                Model.Profiles.Add(new PublishProfileDisplayRow(profile));

            if (projectContext.SelectedProfile != null)
                Model.SelectedProfile = Model.Profiles.FirstOrDefault(p => p.Name == projectContext.SelectedProfile.Name);
        }

        private void CreateNewProfile()
        {
            var projectContext = projectContextMgr.GetContext();

            var profile = new ResourcePackProfileProperties {
                Name = projectContext.Project.Name ?? "New Profile",
                Encoding = {
                    Format = TextureFormat.Format_Lab13,
                },
            };

            var row = new PublishProfileDisplayRow(profile);
            Model.Profiles.Add(row);
            Model.SelectedProfile = row;
        }

        private void CloneSelectedProfile()
        {
            if (!Model.HasSelectedProfile) return;
            var profile = (ResourcePackProfileProperties)Model.SelectedProfile.Profile.Clone();

            var row = new PublishProfileDisplayRow(profile);
            Model.Profiles.Add(row);
            Model.SelectedProfile = row;
        }

        private bool TryGenerateGuid(Guid? currentValue, out Guid id)
        {
            if (!currentValue.HasValue || MessageBox.Show(this, "A value already exists! Are you sure you want to replace it?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {
                id = Guid.NewGuid();
                return true;
            }

            id = Guid.Empty;
            return false;
        }

        #region Events

        private void OnNewProfileClick(object sender, RoutedEventArgs e)
        {
            CreateNewProfile();
        }

        private void OnDuplicateProfileClick(object sender, RoutedEventArgs e)
        {
            CloneSelectedProfile();
        }

        private void OnDeleteProfileClick(object sender, RoutedEventArgs e)
        {
            //var result = MessageBox.Show(this, "Are you sure? This operation cannot be undone.", "Warning", MessageBoxButton.OKCancel);
            //if (result != MessageBoxResult.OK) return;

            Model.Profiles.Remove(Model.SelectedProfile);
        }

        private void OnProfileListBoxMouseDown(object sender, MouseButtonEventArgs e)
        {
            var r = VisualTreeHelper.HitTest(ProfileListBox, e.GetPosition(ProfileListBox));
            if (r.VisualHit.GetType() != typeof(ListBoxItem)) ProfileListBox.UnselectAll();
        }

        private void OnGenerateHeaderUuid(object sender, RoutedEventArgs e)
        {
            var profile = Model.SelectedProfile;
            if (profile == null) return;

            if (TryGenerateGuid(profile.PackHeaderUuid, out var newGuid))
                profile.PackHeaderUuid = newGuid;
        }

        private void OnGenerateModuleUuid(object sender, RoutedEventArgs e)
        {
            var profile = Model.SelectedProfile;
            if (profile == null) return;

            if (TryGenerateGuid(profile.PackModuleUuid, out var newGuid))
                profile.PackModuleUuid = newGuid;
        }

        private void OnEncodingSamplerKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete) return;
            Model.EditEncodingSampler = null;
        }

        private void OnImageEncodingKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete) return;
            Model.EditImageEncoding = null;
        }

        private void OnEditEncodingClick(object sender, RoutedEventArgs e)
        {
            var profile = Model.SelectedProfile?.Profile;
            if (profile == null) return;

            var formatFactory = TextureFormat.GetFactory(Model.SelectedProfile.TextureFormat);

            var window = new TextureFormatWindow {
                Owner = this,
                Model = {
                    Encoding = (ResourcePackEncoding)profile.Encoding.Clone(),
                    DefaultEncoding = formatFactory.Create(),
                    DefaultSampler = Model.EditEncodingSampler,
                    EnableSampler = true,
                },
            };

            if (window.ShowDialog() != true) return;

            profile.Encoding = (ResourcePackOutputProperties)window.Model.Encoding;
        }

        private async void OnSaveButtonClick(object sender, RoutedEventArgs e)
        {
            var context = projectContextMgr.GetContext();

            context.Project.Profiles = Model.Profiles.Select(p => p.Profile).ToList();
            context.SelectedProfile = Model.SelectedProfile?.Profile;

            await projectContextMgr.SaveAsync();

            await Dispatcher.BeginInvoke(() => DialogResult = true);
        }

        #endregion
    }
}
