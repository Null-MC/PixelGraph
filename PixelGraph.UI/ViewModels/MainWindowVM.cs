using PixelGraph.Common;
using PixelGraph.Common.Encoding;
using PixelGraph.Common.Textures;
using System;
using System.Windows;

namespace PixelGraph.UI.ViewModels
{
    internal class MainWindowVM : ViewModelBase
    {
        private EncodingProperties _encoding;

        public event EventHandler Changed;

        public PackProperties Pack {get; private set;}

        private string _packFilename;
        public string PackFilename {
            get => _packFilename;
            set {
                _packFilename = value;
                OnPropertyChanged();
            }
        }

        public Visibility EditVisibility => GetVisibility(Pack != null);

        public string GameEdition {
            get => Pack?.PackEdition;
            set {
                if (Pack == null) return;
                Pack.PackEdition = value;
                OnPropertyChanged();
                OnChanged();
            }
        }

        public int PackFormat {
            get => Pack?.PackFormat ?? 0;
            set {
                if (Pack == null) return;
                Pack.PackFormat = value;
                OnPropertyChanged();
                OnChanged();
            }
        }

        public string PackDescription {
            get => Pack?.PackDescription;
            set {
                if (Pack == null) return;
                Pack.PackDescription = value;
                OnPropertyChanged();
                OnChanged();
            }
        }

        public string PackTags {
            get => Pack?.PackTags;
            set {
                if (Pack == null) return;
                Pack.PackTags = value;
                OnPropertyChanged();
                OnChanged();
            }
        }

        public string InputFormat {
            get => Pack?.InputFormat;
            set {
                if (Pack == null) return;
                Pack.InputFormat = value;
                OnPropertyChanged();
                OnChanged();
            }
        }

        public string OutputFormat {
            get => Pack?.OutputFormat;
            set {
                if (Pack == null) return;
                Pack.OutputFormat = value;
                OnPropertyChanged();
                OnChanged();
            }
        }

        private string _encodingTag = TextureTags.Albedo;
        public string EncodingTag {
            get => _encodingTag;
            set {
                _encodingTag = value;
                OnPropertyChanged();
                UpdateInputOutputProperties();
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

        public string InputRed {
            get => _encoding?.GetInput(_encodingTag, ColorChannel.Red);
            set {
                if (Pack == null) return;
                Pack.SetInput(_encodingTag, ColorChannel.Red, value);
                _encoding.Build(Pack);
                OnPropertyChanged();
                OnChanged();
            }
        }

        public string InputGreen {
            get => _encoding?.GetInput(_encodingTag, ColorChannel.Green);
            set {
                if (Pack == null) return;
                Pack.SetInput(_encodingTag, ColorChannel.Green, value);
                _encoding.Build(Pack);
                OnPropertyChanged();
                OnChanged();
            }
        }

        public string InputBlue {
            get => _encoding?.GetInput(_encodingTag, ColorChannel.Blue);
            set {
                if (Pack == null) return;
                Pack.SetInput(_encodingTag, ColorChannel.Blue, value);
                _encoding.Build(Pack);
                OnPropertyChanged();
                OnChanged();
            }
        }

        public string InputAlpha {
            get => _encoding?.GetInput(_encodingTag, ColorChannel.Alpha);
            set {
                if (Pack == null) return;
                Pack.SetInput(_encodingTag, ColorChannel.Alpha, value);
                _encoding.Build(Pack);
                OnPropertyChanged();
                OnChanged();
            }
        }

        public string OutputRed {
            get => _encoding?.GetOutput(_encodingTag, ColorChannel.Red);
            set {
                if (Pack == null) return;
                Pack.SetOutput(_encodingTag, ColorChannel.Red, value);
                _encoding.Build(Pack);
                OnPropertyChanged();
                OnChanged();
            }
        }

        public string OutputGreen {
            get => _encoding?.GetOutput(_encodingTag, ColorChannel.Green);
            set {
                if (Pack == null) return;
                Pack.SetOutput(_encodingTag, ColorChannel.Green, value);
                _encoding.Build(Pack);
                OnPropertyChanged();
                OnChanged();
            }
        }

        public string OutputBlue {
            get => _encoding?.GetOutput(_encodingTag, ColorChannel.Blue);
            set {
                if (Pack == null) return;
                Pack.SetOutput(_encodingTag, ColorChannel.Blue, value);
                _encoding.Build(Pack);
                OnPropertyChanged();
                OnChanged();
            }
        }

        public string OutputAlpha {
            get => _encoding?.GetOutput(_encodingTag, ColorChannel.Alpha);
            set {
                if (Pack == null) return;
                Pack.SetOutput(_encodingTag, ColorChannel.Alpha, value);
                _encoding.Build(Pack);
                OnPropertyChanged();
                OnChanged();
            }
        }

        public void Initialize(PackProperties pack)
        {
            Pack = pack;
            _encoding = new EncodingProperties();
            _encoding.Build(pack);

            OnPropertyChanged(nameof(EditVisibility));
            OnPropertyChanged(nameof(GameEdition));
            OnPropertyChanged(nameof(PackFormat));
            OnPropertyChanged(nameof(PackDescription));
            OnPropertyChanged(nameof(PackTags));
            OnPropertyChanged(nameof(InputFormat));
            OnPropertyChanged(nameof(OutputFormat));

            UpdateInputOutputProperties();
        }

        private void UpdateInputOutputProperties()
        {
            OnPropertyChanged(nameof(ExportTexture));
            OnPropertyChanged(nameof(InputRed));
            OnPropertyChanged(nameof(InputGreen));
            OnPropertyChanged(nameof(InputBlue));
            OnPropertyChanged(nameof(InputAlpha));
            OnPropertyChanged(nameof(OutputRed));
            OnPropertyChanged(nameof(OutputGreen));
            OnPropertyChanged(nameof(OutputBlue));
            OnPropertyChanged(nameof(OutputAlpha));
        }

        private void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    internal class MainWindowDVM : MainWindowVM
    {
        public MainWindowDVM()
        {
            var pack = new PackProperties {
                Properties = {
                    ["input.format"] = "default",
                    ["output.format"] = "default",
                }
            };

            Initialize(pack);
        }
    }
}
