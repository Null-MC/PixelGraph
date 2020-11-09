using PixelGraph.Common;
using PixelGraph.Common.Encoding;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixelGraph.UI.ViewModels
{
    internal class MainWindowVM : ViewModelBase
    {
        //private readonly CollectionViewSource textureView;
        private string _packFilename;
        private string _textureSearch;

        public event EventHandler Changed;

        public PackEncodingVM Encoding {get;}
        public PackProperties Pack {get; private set;}
        public ObservableCollection<TextureVM> TextureList {get;}
        public IEnumerable<TextureVM> TextureView => FilterTextures();
        public Visibility EditVisibility => GetVisibility(Pack != null);

        public string PackFilename {
            get => _packFilename;
            set {
                _packFilename = value;
                OnPropertyChanged();
            }
        }

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

        public string TextureSearch {
            get => _textureSearch;
            set {
                _textureSearch = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TextureView));
            }
        }

        private TextureVM _currentTexture;
        public TextureVM CurrentTexture {
            get => _currentTexture;
            set {
                _currentTexture = value;
                TexturePreviewImage = value?.AlbedoSource;
                OnPropertyChanged();
            }
        }

        private ImageSource _texturePreviewImage;
        public ImageSource TexturePreviewImage {
            get => _texturePreviewImage;
            set {
                _texturePreviewImage = value;
                OnPropertyChanged();
            }
        }


        public MainWindowVM()
        {
            Encoding = new PackEncodingVM();
            Encoding.Changed += (o, e) => OnChanged();

            TextureList = new ObservableCollection<TextureVM>();
            TextureList.CollectionChanged += TextureList_OnCollectionChanged;

            //textureView = new CollectionViewSource {
            //    Source = TextureList,
            //};

            //textureView.Filter += TextureView_OnFilter;
        }

        //private void TextureView_OnFilter(object sender, FilterEventArgs e)
        //{
        //    //
        //}

        private IEnumerable<TextureVM> FilterTextures()
        {
            return string.IsNullOrWhiteSpace(TextureSearch) ? TextureList
                : TextureList.Where(x => x.Name.Contains(TextureSearch, StringComparison.InvariantCultureIgnoreCase));
        }

        public void Initialize(PackProperties pack)
        {
            Pack = pack;
            Encoding.Pack = pack;

            OnPropertyChanged(nameof(EditVisibility));
            OnPropertyChanged(nameof(GameEdition));
            OnPropertyChanged(nameof(PackFormat));
            OnPropertyChanged(nameof(PackDescription));
            OnPropertyChanged(nameof(PackTags));
        }

        private void TextureList_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(TextureView));
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
                    ["input.format"] = EncodingProperties.Raw,
                    ["output.format"] = EncodingProperties.Lab13,
                }
            };

            Initialize(pack);

            TextureSearch = "as";
            TextureList.Add(new TextureVM(new PbrProperties {Name = "Dirt"}));
            TextureList.Add(new TextureVM(new PbrProperties {Name = "Grass"}));
            TextureList.Add(new TextureVM(new PbrProperties {Name = "Glass"}));
            TextureList.Add(new TextureVM(new PbrProperties {Name = "Stone"}));
        }
    }
}
