namespace PixelGraph.UI.Internal.Settings
{
    public class AppSettingsDataModel
    {
        public const string DefaultImageEditorExe = "mspaint";
        public const string DefaultImageEditorArgs = "\"$1\"";
        public const string DefaultThemeBaseColor = "dark";
        public const string DefaultThemeAccentColor = "emerald";

        public string SelectedPublishLocation {get; set;}
        public bool PublishCloseOnComplete {get; set;}
        public string TextureEditorExecutable {get; set;}
        public string TextureEditorArguments {get; set;}

        public string ThemeBaseColor {get; set;}
        public string ThemeAccentColor {get; set;}

        public RenderPreviewSettings RenderPreview {get; set;}


        public AppSettingsDataModel()
        {
            TextureEditorExecutable = DefaultImageEditorExe;
            TextureEditorArguments = DefaultImageEditorArgs;
            ThemeBaseColor = DefaultThemeBaseColor;
            ThemeAccentColor = DefaultThemeAccentColor;

            RenderPreview = new RenderPreviewSettings();
        }
    }
}
