using PixelGraph.Common;
using PixelGraph.UI.Internal;
using System;

namespace PixelGraph.UI.ViewModels
{
    public class AboutViewModel : ModelBase
    {
        public string VersionText {get;}
        public string CopyrightText {get;}


        public AboutViewModel()
        {
            VersionText = $"Version {AppCommon.Version}";
            CopyrightText = $"Copyright © {DateTime.Now.Year} Joshua Miller\nAll Rights Reserved";
        }
    }

    //public class AboutDesignerViewModel : AboutViewModel
    //{
    //    public AboutDesignerViewModel() {}
    //}
}
