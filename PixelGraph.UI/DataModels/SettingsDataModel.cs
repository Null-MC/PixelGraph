namespace PixelGraph.UI.DataModels
{
    public class SettingsDataModel
    {
        public string SelectedPublishLocation {get; set;}
        public bool PublishCloseOnComplete {get; set;}
        public string TextureEditCommand {get; set;}

        public string AppThemeBase {get; set;}
        public string AppThemeAccent {get; set;}


        public SettingsDataModel()
        {
            TextureEditCommand = "mspaint.exe \"$1\"";
            AppThemeBase = "dark";
            AppThemeAccent = "emerald";
        }
    }
}
