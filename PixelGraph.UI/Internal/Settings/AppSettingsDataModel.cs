using System;

namespace PixelGraph.UI.Internal.Settings;

public class AppSettingsDataModel : ICloneable
{
    public static int CurrentLicenseVersion = 2;
    public static int CurrentTermsVersion = 2;

    public const string DefaultImageEditorExe = "mspaint";
    public const string DefaultImageEditorArgs = "\"$1\"";
    public const string DefaultThemeBaseColor = "dark";
    public const string DefaultThemeAccentColor = "emerald";
    public const int DefaultMaxRecentProjects = 12;

    public int? AcceptedLicenseAgreementVersion {get; set;}
    public int? AcceptedTermsOfServiceVersion {get; set;}
    public bool? HasAcceptedPatreonNotification {get; set;}
    public int? MaxRecentProjects {get; set;}
    public int? Concurrency {get; set;}

    public string SelectedPublishLocation {get; set;}
    public bool PublishCloseOnComplete {get; set;}

    public string TextureEditorExecutable {get; set;}
    public string TextureEditorArguments {get; set;}

    public string ThemeBaseColor {get; set;}
    public string ThemeAccentColor {get; set;}

    public RenderPreviewSettings RenderPreview {get; private set;}


    public AppSettingsDataModel()
    {
        TextureEditorExecutable = DefaultImageEditorExe;
        TextureEditorArguments = DefaultImageEditorArgs;
        ThemeBaseColor = DefaultThemeBaseColor;
        ThemeAccentColor = DefaultThemeAccentColor;
        MaxRecentProjects = DefaultMaxRecentProjects;

        RenderPreview = new RenderPreviewSettings();
    }

    public object Clone()
    {
        var clone = (AppSettingsDataModel)MemberwiseClone();
        clone.RenderPreview = (RenderPreviewSettings)RenderPreview.Clone();
        return clone;
    }
}