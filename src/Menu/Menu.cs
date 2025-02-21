using CounterStrikeSharp.API.Core;

public partial class Menu
{
    private static Plugin Instance = Plugin.Instance;

    public static void Open(CCSPlayerController player)
    {
        switch (Instance.Config.Settings.Menu.ToLower())
        {
            case "chat":
            case "text":
                Chat.Open(player);
                break;
            case "html":
            case "center":
            case "centerhtml":
            case "hud":
                HTML.Open(player);
                break;
            case "wasd":
            case "wasdmenu":
                WASD.Open(player);
                break;
            case "screen":
            case "screenmenu":
                Screen.Open(player);
                break;
            default:
                HTML.Open(player);
                break;
        }
    }
}