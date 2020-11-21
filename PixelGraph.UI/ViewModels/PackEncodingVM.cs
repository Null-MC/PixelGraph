using PixelGraph.Common.Encoding;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System;
using System.ComponentModel;
using System.Linq;

namespace PixelGraph.UI.ViewModels
{
    internal class PackEncodingVM : ViewModelBase
    {
        private ResourcePackInputProperties _packInput;
        private ResourcePackProfileProperties _packProfile;
        private TextureEncoding _inputEncoding;
        private TextureOutputEncoding _outputEncoding;
        private string _selectedTag;

        public event EventHandler Changed;

        public string InputRedDefault => GetInputDefaultText(ColorChannel.Red);
        public string InputGreenDefault => GetInputDefaultText(ColorChannel.Green);
        public string InputBlueDefault => GetInputDefaultText(ColorChannel.Blue);
        public string InputAlphaDefault => GetInputDefaultText(ColorChannel.Alpha);

        public string OutputRedDefault => GetOutputDefaultText(ColorChannel.Red);
        public string OutputGreenDefault => GetOutputDefaultText(ColorChannel.Green);
        public string OutputBlueDefault => GetOutputDefaultText(ColorChannel.Blue);
        public string OutputAlphaDefault => GetOutputDefaultText(ColorChannel.Alpha);

        public ResourcePackInputProperties PackInput {
            get => _packInput;
            set {
                _packInput = value;
                OnPropertyChanged();

                UpdateInput();
                UpdateDefaultProperties();
            }
        }

        public ResourcePackProfileProperties PackProfile {
            get => _packProfile;
            set {
                _packProfile = value;
                OnPropertyChanged();

                UpdateOutput();
                UpdateDefaultProperties();
            }
        }

        public string SelectedTag {
            get => _selectedTag;
            set {
                _selectedTag = value;
                OnPropertyChanged();

                UpdateInput();
                UpdateOutput();
                UpdateDefaultProperties();
            }
        }

        public TextureEncoding SelectedInput {
            get => _inputEncoding;
            set {
                _inputEncoding = value;
                OnPropertyChanged();
            }
        }

        public TextureOutputEncoding SelectedOutput {
            get => _outputEncoding;
            set {
                _outputEncoding = value;
                OnPropertyChanged();
            }
        }


        public PackEncodingVM()
        {
            _selectedTag = TextureTags.Albedo;
        }

        private void UpdateInput()
        {
            var newInput = _packInput.GetRawEncoding(_selectedTag);

            if (newInput != _inputEncoding) {
                if (_inputEncoding != null) _inputEncoding.PropertyChanged -= OnEncodingPropertyChanged;
                if (newInput != null) newInput.PropertyChanged += OnEncodingPropertyChanged;
                SelectedInput = newInput;
            }
        }

        private void UpdateOutput()
        {
            var newOutput = _packProfile.Output.GetRawTextureEncoding(_selectedTag);

            if (newOutput != _outputEncoding) {
                if (_outputEncoding != null) _outputEncoding.PropertyChanged -= OnEncodingPropertyChanged;
                if (newOutput != null) newOutput.PropertyChanged += OnEncodingPropertyChanged;
                SelectedOutput = newOutput;
            }
        }

        private string GetInputDefaultText(ColorChannel colorChannel)
        {
            string defaultValue = null;

            var encoding = _packInput.GetRawEncoding(_selectedTag);
            var channel = encoding.GetEncodingChannel(colorChannel)
                ?? defaultValue; // TODO: Get format default

            return channel != null ? GetLabelText(channel) : null;
        }

        private string GetOutputDefaultText(ColorChannel colorChannel)
        {
            string defaultValue = null;

            var encoding = _packInput.GetRawEncoding(_selectedTag);
            var channel = encoding?.GetEncodingChannel(colorChannel)
                ?? defaultValue; // TODO: Get format default

            return channel != null ? GetLabelText(channel) : null;
        }

        private void UpdateDefaultProperties()
        {
            OnPropertyChanged(nameof(InputRedDefault));
            OnPropertyChanged(nameof(InputGreenDefault));
            OnPropertyChanged(nameof(InputBlueDefault));
            OnPropertyChanged(nameof(InputAlphaDefault));
            OnPropertyChanged(nameof(OutputRedDefault));
            OnPropertyChanged(nameof(OutputGreenDefault));
            OnPropertyChanged(nameof(OutputBlueDefault));
            OnPropertyChanged(nameof(OutputAlphaDefault));
        }

        private void OnEncodingPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        private static string GetLabelText(string encodingChannel)
        {
            return new EncodingChannelValues().FirstOrDefault(x => EncodingChannel.Is(x.Value, encodingChannel))?.Text;
        }
    }
}
