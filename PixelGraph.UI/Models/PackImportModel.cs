using Microsoft.Extensions.Logging;
using PixelGraph.Common;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.UI.Internal;
using PixelGraph.UI.ViewModels;
using System;
using PixelGraph.UI.Internal.Logging;

namespace PixelGraph.UI.Models
{
    internal class PackImportModel : ModelBase
    {
        private ImportTreeNode _rootNode;
        private string _importSource;
        private bool _copyUntracked;
        private bool _includeUnknown;
        private volatile bool _isReady;
        private volatile bool _isActive;
        private volatile bool _showLog;

        public event EventHandler IncludeUnknownChanged;
        public event EventHandler<LogEventArgs> LogEvent;

        public bool IsArchive {get; set;}
        public bool AsGlobal {get; set;}
        public GameEditions SourceGameEdition {get; set;}
        public string SourceTextureFormat {get; set;}
        public PackOutputEncoding Encoding {get; set;}

        public bool IsReady {
            get => _isReady;
            set {
                _isReady = value;
                OnPropertyChanged();
            }
        }

        public string ImportSource {
            get => _importSource;
            set {
                _importSource = value;
                OnPropertyChanged();
            }
        }

        public ImportTreeNode RootNode {
            get => _rootNode;
            set {
                _rootNode = value;
                OnPropertyChanged();
            }
        }

        public bool CopyUntracked {
            get => _copyUntracked;
            set {
                _copyUntracked = value;
                OnPropertyChanged();

                OnIncludeUnknownChanged();
            }
        }

        public bool IncludeUnknown {
            get => _includeUnknown;
            set {
                _includeUnknown = value;
                OnPropertyChanged();

                OnIncludeUnknownChanged();
            }
        }

        public bool IsActive {
            get => _isActive;
            set {
                _isActive = value;
                OnPropertyChanged();
            }
        }

        public bool ShowLog {
            get => _showLog;
            set {
                _showLog = value;
                OnPropertyChanged();
            }
        }


        public PackImportModel()
        {
            Encoding = new PackOutputEncoding();

            SourceGameEdition = GameEditions.Java;
            SourceTextureFormat = TextureFormat.Format_Color;
            CopyUntracked = true;
            IncludeUnknown = false;
            AsGlobal = false;
        }

        protected void AppendLog(LogLevel level, string text)
        {
            var e = new LogEventArgs(level, text);
            LogEvent?.Invoke(this, e);
        }

        private void OnIncludeUnknownChanged()
        {
            IncludeUnknownChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    internal class ImportPackDesignVM : PackImportModel
    {
        public ImportPackDesignVM()
        {
            ImportSource = "C:\\SomePath\\File.zip";
            IsArchive = true;

            AppendLog(LogLevel.Information, "Hello World!");

            ShowLog = false;
        }
    }
}
