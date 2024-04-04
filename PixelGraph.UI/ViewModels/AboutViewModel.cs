using PixelGraph.Common;
using PixelGraph.UI.Internal;

namespace PixelGraph.UI.ViewModels;

public class AboutViewModel : ModelBase
{
    public string VersionText {get;} = $"Version {AppCommon.Version}";
    public string CopyrightText {get;} = $"Copyright © {DateTime.Now.Year} Joshua Miller\nAll Rights Reserved";
}
