using PixelGraph.Common.Encoding;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.Textures;
using System;

namespace PixelGraph.UI.ViewModels
{
    internal class InputVM : ViewModelBase
    {
        private ResourcePackInputProperties _packInput;
        private TextureEncoding _selectedEncoding;
        private string _selectedTag;

        public event EventHandler DataChanged;

        public string RootDirectory {get; set;}
        public TextureEncoding DefaultEncoding {get; private set;}

        public ResourcePackInputProperties PackInput {
            get => _packInput;
            set {
                _packInput = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(Format));
                PopulateEncoding();
            }
        }

        public string SelectedTag {
            get => _selectedTag;
            set {
                _selectedTag = value;
                OnPropertyChanged();

                PopulateEncoding();
            }
        }

        public string Format {
            get => _packInput?.Format;
            set {
                if (_packInput == null) return;
                _packInput.Format = value;
                OnPropertyChanged();

                OnDataChanged();
                PopulateEncoding();
            }
        }

        public string RedValue {
            get => _selectedEncoding?.Red;
            set {
                if (_selectedEncoding == null) return;
                _selectedEncoding.Red = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public string GreenValue {
            get => _selectedEncoding?.Green;
            set {
                if (_selectedEncoding == null) return;
                _selectedEncoding.Green = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public string BlueValue {
            get => _selectedEncoding?.Blue;
            set {
                if (_selectedEncoding == null) return;
                _selectedEncoding.Blue = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }

        public string AlphaValue {
            get => _selectedEncoding?.Alpha;
            set {
                if (_selectedEncoding == null) return;
                _selectedEncoding.Alpha = value;
                OnPropertyChanged();
                OnDataChanged();
            }
        }


        public InputVM()
        {
            _selectedTag = TextureTags.Albedo;
        }

        private void PopulateEncoding()
        {
            DefaultEncoding = TextureEncoding.GetDefault(Format, _selectedTag);
            _selectedEncoding = _packInput.GetRawEncoding(_selectedTag);

            OnPropertyChanged(nameof(DefaultEncoding));
            OnPropertyChanged(nameof(RedValue));
            OnPropertyChanged(nameof(GreenValue));
            OnPropertyChanged(nameof(BlueValue));
            OnPropertyChanged(nameof(AlphaValue));
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    internal class InputDesignVM : InputVM
    {
        public InputDesignVM()
        {
            PackInput = new ResourcePackInputProperties {
                Format = TextureEncoding.Format_Default,
                Albedo = {
                    Red = EncodingChannel.Blue,
                }
            };
        }
    }
}
