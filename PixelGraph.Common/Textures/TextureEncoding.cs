using PixelGraph.Common.Encoding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PixelGraph.Common.Textures
{
    public class TextureEncoding : INotifyPropertyChanged
    {
        public const string Format_Raw = "raw";
        public const string Format_Default = "default";
        public const string Format_Legacy = "legacy";
        public const string Format_Lab11 = "lab-1.1";
        public const string Format_Lab13 = "lab-1.3";

        private string _red, _green, _blue, _alpha;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Red {
            get => _red;
            set {
                _red = value;
                OnPropertyChanged();
            }
        }

        public string Green {
            get => _green;
            set {
                _green = value;
                OnPropertyChanged();
            }
        }

        public string Blue {
            get => _blue;
            set {
                _blue = value;
                OnPropertyChanged();
            }
        }

        public string Alpha {
            get => _alpha;
            set {
                _alpha = value;
                OnPropertyChanged();
            }
        }

        
        public string GetEncodingChannel(ColorChannel color)
        {
            return color switch {
                ColorChannel.Red => _red,
                ColorChannel.Green => _green,
                ColorChannel.Blue => _blue,
                ColorChannel.Alpha => _alpha,
                _ => null,
            };
        }

        public ColorChannel GetColorChannel(string encodingChannel) => GetColorChannels(encodingChannel).FirstOrDefault();

        public IEnumerable<ColorChannel> GetColorChannels(string encodingChannel)
        {
            if (string.Equals(Red, encodingChannel, StringComparison.InvariantCultureIgnoreCase)) yield return ColorChannel.Red;
            if (string.Equals(Green, encodingChannel, StringComparison.InvariantCultureIgnoreCase)) yield return ColorChannel.Green;
            if (string.Equals(Blue, encodingChannel, StringComparison.InvariantCultureIgnoreCase)) yield return ColorChannel.Blue;
            if (string.Equals(Alpha, encodingChannel, StringComparison.InvariantCultureIgnoreCase)) yield return ColorChannel.Alpha;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static TextureOutputEncoding GetDefault(string format, string tag)
        {
            return GetFormat(format)?.Get(tag);
        }

        private static TextureFormatBase GetFormat(string format)
        {
            if (format == null) return null;
            return formatMap.TryGetValue(format, out var textureFormat) ? textureFormat : null;
        }

        private static readonly Dictionary<string, TextureFormatBase> formatMap =
            new Dictionary<string, TextureFormatBase>(StringComparer.InvariantCultureIgnoreCase) {
                [Format_Raw] = new RawEncoding(),
                [Format_Default] = new DefaultEncoding(),
                [Format_Legacy] = new LegacyEncoding(),
                [Format_Lab11] = new Lab11Encoding(),
                [Format_Lab13] = new Lab13Encoding(),
            };
    }

    public class TextureOutputEncoding : TextureEncoding
    {
        private bool? _include;
        private string _sampler;

        public bool? Include {
            get => _include;
            set {
                _include = value;
                OnPropertyChanged();
            }
        }

        public string Sampler {
            get => _sampler;
            set {
                _sampler = value;
                OnPropertyChanged();
            }
        }
    }
}
