using ControlzEx.Theming;
using PixelGraph.UI.Internal.Settings;
using System.Windows;

namespace PixelGraph.UI.Internal.Utilities;

public interface IThemeHelper
{
    void ApplyCurrent(FrameworkElement target);
}

internal class ThemeHelper : IThemeHelper
{
    private readonly IAppSettingsManager settings;


    public ThemeHelper(IAppSettingsManager settings)
    {
        this.settings = settings;
    }

    public void ApplyCurrent(FrameworkElement target)
    {
        var name = $"{settings.Data.ThemeBaseColor}.{settings.Data.ThemeAccentColor}";
        ThemeManager.Current.ChangeTheme(target, name);
    }
}