using CounterStrikeSharp.API.Core;

public partial class Menu
{
    private static Plugin Instance = Plugin.Instance;

    public static void Open(CCSPlayerController player)
    {
        if (Instance.Config.Settings.ScreenMenu)
            Screen.Open(player);

        else MenuManager.Open(player);
    }
}