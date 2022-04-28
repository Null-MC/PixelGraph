using PixelGraph.Common;
using PixelGraph.UI.Internal;

namespace PixelGraph.UI.ViewModels
{
    public class AboutViewModel : ModelBase
    {
        protected string _versionText;

        public string VersionText {
            get => _versionText;
            private set {
                _versionText = value;
                OnPropertyChanged();
            }
        }


        public void Initialize()
        {
            VersionText = $"Version {AppCommon.Version}";
        }
    }

    public class AboutDesignerViewModel : AboutViewModel
    {
        public AboutDesignerViewModel()
        {
            _versionText = "Version 1.2.3-beta";
        }
    }
}
