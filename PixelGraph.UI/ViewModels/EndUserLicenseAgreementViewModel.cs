using Microsoft.Extensions.DependencyInjection;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Settings;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace PixelGraph.UI.ViewModels
{
    internal class EndUserLicenseAgreementViewModel : ModelBase
    {
        private IAppSettings appSettings;
        private bool _hasNotAccepted;

        public bool HasNotAccepted {
            get => _hasNotAccepted;
            set {
                _hasNotAccepted = value;
                OnPropertyChanged();
            }
        }


        public void Initialize(IServiceProvider provider)
        {
            appSettings = provider.GetRequiredService<IAppSettings>();
            HasNotAccepted = !(appSettings.Data.HasAcceptedTermsOfService ?? false);
        }

        public async Task SetResultAsync(bool result)
        {
            appSettings.Data.HasAcceptedLicenseAgreement = result;
            await appSettings.SaveAsync();
        }

        public FlowDocument LoadDocument()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("PixelGraph.UI.Resources.EULA.rtf");
            if (stream == null) throw new ApplicationException("Failed to load EULA document!");

            var document = new FlowDocument();
            document.BeginInit();
            document.TextAlignment = TextAlignment.Justify;
            var range = new TextRange(document.ContentStart, document.ContentEnd);
            range.Load(stream, DataFormats.Rtf);
            document.EndInit();
            
            return document;
        }
    }
}
