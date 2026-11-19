using MudBlazor;

namespace JokerBlazor;

public static class AppTheme
{
    public static MudTheme Create()
    {
        var theme = new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#4F46E5",
                PrimaryContrastText = "#FFFFFF",
                Secondary = "#0F766E",
                AppbarBackground = "#312E81",
                AppbarText = "#FFFFFF",
                Background = "#F3F4FB",
                Surface = "#FFFFFF",
                DrawerBackground = "#FFFFFF",
                DrawerText = "#1C1B22",
                DrawerIcon = "#4338CA",
                TextPrimary = "#1C1B22",
                TextSecondary = "#4B4A57",
                ActionDefault = "#4338CA",
                LinesDefault = "#E4E5F0"
            },
            PaletteDark = new PaletteDark
            {
                Primary = "#A5B4FC",
                PrimaryContrastText = "#1E1B4B",
                Secondary = "#5EEAD4",
                AppbarBackground = "#12121A",
                AppbarText = "#F4F4F8",
                Background = "#101018",
                Surface = "#1C1C28",
                DrawerBackground = "#16161F",
                DrawerText = "#F4F4F8",
                DrawerIcon = "#C7D2FE",
                TextPrimary = "#F4F4F8",
                TextSecondary = "#C6C6D2",
                ActionDefault = "#C7D2FE",
                LinesDefault = "#2E2E3D"
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "14px",
                DrawerWidthLeft = "248px",
                AppbarHeight = "64px"
            }
        };

        var sans = new[] { "Source Sans 3", "Segoe UI", "sans-serif" };
        var display = new[] { "Fraunces", "Georgia", "serif" };
        theme.Typography.Default.FontFamily = sans;
        theme.Typography.Body1.FontFamily = sans;
        theme.Typography.Body2.FontFamily = sans;
        theme.Typography.Button.FontFamily = sans;
        theme.Typography.Button.FontWeight = "600";
        theme.Typography.Caption.FontFamily = sans;
        theme.Typography.Overline.FontFamily = sans;
        theme.Typography.Subtitle1.FontFamily = sans;
        theme.Typography.Subtitle2.FontFamily = sans;
        theme.Typography.H3.FontFamily = display;
        theme.Typography.H3.FontWeight = "560";
        theme.Typography.H4.FontFamily = display;
        theme.Typography.H4.FontWeight = "560";
        theme.Typography.H5.FontFamily = display;
        theme.Typography.H5.FontWeight = "560";
        theme.Typography.H6.FontFamily = sans;
        theme.Typography.H6.FontWeight = "700";
        return theme;
    }
}
