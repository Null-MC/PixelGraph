using Microsoft.Extensions.Logging;
using PixelGraph.Common.Projects;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Logging;
using PixelGraph.UI.ViewModels;

namespace PixelGraph.UI.Models;

internal class PackConvertModel : ModelBase
{
    private ImportTreeNode? _rootNode;
    private string? _rootDirectory;
    private string? _importSource;
    //private bool _copyUntracked;
    //private bool _includeUnknown;
    private volatile bool _isReady;
    private volatile bool _isActive;
    private volatile bool _showLog;

    //public event EventHandler IncludeUnknownChanged;
    public event EventHandler<LogEventArgs>? LogEvent;

    public bool IsArchive {get; set;}
    //public bool AsGlobal {get; set;}
    public string SourceFormat {get; set;}
    public string OutputFormat {get; set;}
    public IProjectDescription? Project {get; set;}
    public PackOutputEncoding PackOutput {get; set;}
    //public ResourcePackProfileProperties PackOutput {get; set;}

    public bool IsReady {
        get => _isReady;
        set {
            _isReady = value;
            OnPropertyChanged();
        }
    }

    public string? RootDirectory {
        get => _rootDirectory;
        set {
            _rootDirectory = value;
            OnPropertyChanged();
        }
    }

    public string? ImportSource {
        get => _importSource;
        set {
            _importSource = value;
            OnPropertyChanged();
        }
    }

    public ImportTreeNode? RootNode {
        get => _rootNode;
        set {
            _rootNode = value;
            OnPropertyChanged();
        }
    }

    //public bool CopyUntracked {
    //    get => _copyUntracked;
    //    set {
    //        _copyUntracked = value;
    //        OnPropertyChanged();

    //        OnIncludeUnknownChanged();
    //    }
    //}

    //public bool IncludeUnknown {
    //    get => _includeUnknown;
    //    set {
    //        _includeUnknown = value;
    //        OnPropertyChanged();

    //        OnIncludeUnknownChanged();
    //    }
    //}

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


    public PackConvertModel()
    {
        PackOutput = new PackOutputEncoding();

        SourceFormat = TextureFormat.Format_Color;
        OutputFormat = TextureFormat.Format_Color;
        //CopyUntracked = true;
        //IncludeUnknown = false;
        //AsGlobal = false;
    }

    public void AppendLog(LogLevel level, string text)
    {
        var e = new LogEventArgs(level, text);
        LogEvent?.Invoke(this, e);
    }

    //private void OnIncludeUnknownChanged()
    //{
    //    IncludeUnknownChanged?.Invoke(this, EventArgs.Empty);
    //}
}

internal class PackConvertDesignModel : PackConvertModel
{
    public PackConvertDesignModel()
    {
        ImportSource = "C:\\SomePath\\File.zip";
        //IsArchive = true;

        AppendLog(LogLevel.Information, "Hello World!");

        ShowLog = false;
    }
}