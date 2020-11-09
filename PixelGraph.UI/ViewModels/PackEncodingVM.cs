using PixelGraph.Common;
using PixelGraph.Common.Encoding;
using PixelGraph.Common.Textures;
using System;
using System.Linq;

namespace PixelGraph.UI.ViewModels
{
    internal class PackEncodingVM : ViewModelBase
    {
        public PackProperties _pack;
        private readonly EncodingProperties _encoding;
        private string _encodingTag;

        public event EventHandler Changed;

        public PackProperties Pack {
            get => _pack;
            set {
                _pack = value;
                UpdateEncodingDefaults();
                UpdateInputOutputProperties();
                OnPropertyChanged(nameof(InputFormat));
                OnPropertyChanged(nameof(OutputFormat));
            }
        }

        public string InputRedDefault => GetInputDefault(_encodingTag, ColorChannel.Red);
        public string InputGreenDefault => GetInputDefault(_encodingTag, ColorChannel.Green);
        public string InputBlueDefault => GetInputDefault(_encodingTag, ColorChannel.Blue);
        public string InputAlphaDefault => GetInputDefault(_encodingTag, ColorChannel.Alpha);
        public string OutputRedDefault => GetOutputDefault(_encodingTag, ColorChannel.Red);
        public string OutputGreenDefault => GetOutputDefault(_encodingTag, ColorChannel.Green);
        public string OutputBlueDefault => GetOutputDefault(_encodingTag, ColorChannel.Blue);
        public string OutputAlphaDefault => GetOutputDefault(_encodingTag, ColorChannel.Alpha);

        public string InputFormat {
            get => Pack?.InputFormat;
            set {
                if (Pack == null) return;
                Pack.InputFormat = value;
                UpdateEncodingDefaults();
                OnPropertyChanged();
                UpdateInputOutputProperties();
                OnChanged();
            }
        }

        public string OutputFormat {
            get => Pack?.OutputFormat;
            set {
                if (Pack == null) return;
                Pack.OutputFormat = value;
                OnPropertyChanged();
                UpdateInputOutputProperties();
                UpdateEncodingDefaults();
                OnChanged();
            }
        }

        public string EncodingTag {
            get => _encodingTag;
            set {
                _encodingTag = value;
                OnPropertyChanged();
                UpdateInputOutputProperties();
                UpdateEncodingProperties();
            }
        }

        public string InputRed {
            get => Pack?.GetInput(_encodingTag, ColorChannel.Red);
            set {
                if (Pack == null) return;
                Pack.SetInput(_encodingTag, ColorChannel.Red, value);
                OnPropertyChanged(nameof(InputRed));
                OnPropertyChanged(nameof(InputRedDefault));
                OnChanged();
            }
        }

        public string InputGreen {
            get => Pack?.GetInput(_encodingTag, ColorChannel.Green);
            set {
                if (Pack == null) return;
                Pack.SetInput(_encodingTag, ColorChannel.Green, value);
                OnPropertyChanged(nameof(InputGreen));
                OnPropertyChanged(nameof(InputGreenDefault));
                OnChanged();
            }
        }

        public string InputBlue {
            get => Pack?.GetInput(_encodingTag, ColorChannel.Blue);
            set {
                if (Pack == null) return;
                Pack.SetInput(_encodingTag, ColorChannel.Blue, value);
                OnPropertyChanged(nameof(InputBlue));
                OnPropertyChanged(nameof(InputBlueDefault));
                OnChanged();
            }
        }

        public string InputAlpha {
            get => Pack?.GetInput(_encodingTag, ColorChannel.Alpha);
            set {
                if (Pack == null) return;
                Pack.SetInput(_encodingTag, ColorChannel.Alpha, value);
                OnPropertyChanged(nameof(InputAlpha));
                OnPropertyChanged(nameof(InputAlphaDefault));
                OnChanged();
            }
        }

        public string OutputRed {
            get => Pack?.GetOutput(_encodingTag, ColorChannel.Red);
            set {
                if (Pack == null) return;
                Pack.SetOutput(_encodingTag, ColorChannel.Red, value);
                OnPropertyChanged(nameof(OutputRed));
                OnPropertyChanged(nameof(OutputRedDefault));
                OnChanged();
            }
        }

        public string OutputGreen {
            get => Pack?.GetOutput(_encodingTag, ColorChannel.Green);
            set {
                if (Pack == null) return;
                Pack.SetOutput(_encodingTag, ColorChannel.Green, value);
                OnPropertyChanged(nameof(OutputGreen));
                OnPropertyChanged(nameof(OutputGreenDefault));
                OnChanged();
            }
        }

        public string OutputBlue {
            get => Pack?.GetOutput(_encodingTag, ColorChannel.Blue);
            set {
                if (Pack == null) return;
                Pack.SetOutput(_encodingTag, ColorChannel.Blue, value);
                OnPropertyChanged(nameof(OutputBlue));
                OnPropertyChanged(nameof(OutputBlueDefault));
                OnChanged();
            }
        }

        public string OutputAlpha {
            get => Pack?.GetOutput(_encodingTag, ColorChannel.Alpha);
            set {
                if (Pack == null) return;
                Pack.SetOutput(_encodingTag, ColorChannel.Alpha, value);
                OnPropertyChanged(nameof(OutputAlpha));
                OnPropertyChanged(nameof(OutputAlphaDefault));
                OnChanged();
            }
        }

        public bool ExportTexture {
            get => _encoding?.GetExported(_encodingTag) ?? false;
            set {
                if (Pack == null) return;
                Pack.SetExported(_encodingTag, value);
                _encoding.Build(Pack);
                OnPropertyChanged();
                OnChanged();
            }
        }


        public PackEncodingVM()
        {
            _encoding = new EncodingProperties();
            _encodingTag = TextureTags.Albedo;
        }

        private void UpdateEncodingDefaults()
        {
            _encoding.Build(new PackProperties {
                Properties = {
                    ["input.format"] = Pack.InputFormat,
                    ["output.format"] = Pack.OutputFormat,
                }
            });

            UpdateEncodingProperties();
        }

        private string GetInputDefault(string textureTag, ColorChannel colorChannel)
        {
            var key = _encoding?.GetInput(textureTag, colorChannel);
            if (key == null) return null;

            return new EncodingChannelValues()
                .FirstOrDefault(x => string.Equals(x.Value, key, StringComparison.InvariantCultureIgnoreCase))?.Text;
        }

        private string GetOutputDefault(string textureTag, ColorChannel colorChannel)
        {
            var key = _encoding?.GetOutput(textureTag, colorChannel);
            if (key == null) return null;

            return new EncodingChannelValues()
                .FirstOrDefault(x => string.Equals(x.Value, key, StringComparison.InvariantCultureIgnoreCase))?.Text;
        }

        private void UpdateInputOutputProperties()
        {
            OnPropertyChanged(nameof(InputRed));
            OnPropertyChanged(nameof(InputGreen));
            OnPropertyChanged(nameof(InputBlue));
            OnPropertyChanged(nameof(InputAlpha));
            OnPropertyChanged(nameof(OutputRed));
            OnPropertyChanged(nameof(OutputGreen));
            OnPropertyChanged(nameof(OutputBlue));
            OnPropertyChanged(nameof(OutputAlpha));

            OnPropertyChanged(nameof(ExportTexture));
        }

        private void UpdateEncodingProperties()
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

        private void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
