using PixelGraph.Common;
using PixelGraph.Common.Encoding;
using System;
using System.Windows;
using System.Windows.Media;
using PixelGraph.Common.Textures;

namespace PixelGraph.UI.ViewModels
{
    internal class MainWindowVM : ViewModelBase
    {
        private string _packFilename;

        public event EventHandler Changed;

        public PackEncodingVM Encoding {get;}
        public TextureVM Texture {get;}
        public PackProperties Pack {get; private set;}
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

        //private TextureTreeTexture _selectedTexture;
        //public TextureTreeTexture SelectedTexture {
        //    get => _selectedTexture;
        //    set {
        //        _selectedTexture = value;
        //        OnPropertyChanged();
        //    }
        //}

        //private ImageSource _texturePreviewImage;
        //public ImageSource TexturePreviewImage {
        //    get => _texturePreviewImage;
        //    set {
        //        _texturePreviewImage = value;
        //        OnPropertyChanged();
        //    }
        //}


        public MainWindowVM()
        {
            Encoding = new PackEncodingVM();
            Encoding.Changed += (o, e) => OnChanged();

            Texture = new TextureVM();
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

            Texture.Search = "as";
            Texture.TreeRoot.Nodes.Add(new TextureTreeDirectory {
                Name = "assets",
                Nodes = {
                    new TextureTreeDirectory {
                        Name = "minecraft",
                        Nodes = {
                            new TextureTreeTexture {
                                Name = "Dirt",
                            },
                            new TextureTreeTexture {
                                Name = "Grass",
                            },
                            new TextureTreeTexture {
                                Name = "Glass",
                            },
                            new TextureTreeTexture {
                                Name = "Stone",
                            },
                        },
                    },
                },
            });

            Texture.Textures.Add(new TextureSource {
                Tag = TextureTags.Albedo,
                Name = "albedo.png",
                Image = null,
            });

            Texture.Textures.Add(new TextureSource {
                Tag = TextureTags.Normal,
                Name = "normal.png",
                Image = null,
            });
        }
    }
}
