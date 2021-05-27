namespace PixelGraph.UI.DataModels
{
    internal class SettingsDataModel
    {
        public string SelectedPublishLocation {get; set;}
        public bool PublishCloseOnComplete {get; set;}
        public string TextureEditCommand {get; set;}


        public SettingsDataModel()
        {
            TextureEditCommand = "mspaint.exe $1";
        }
    }
}
