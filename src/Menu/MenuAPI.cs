using CS2MenuManager.API.Interface;
using CS2MenuManager.API.Menu;

public static partial class Menu
{
    // thanks schwarper :)
    public static class MenuAPI
    {
        public static IMenu Create(string title, Type menuType, IMenu? prevMenu = null)
        {
            IMenu menu = MenuByType(menuType, title);

            if (prevMenu != null)
                menu.PrevMenu = prevMenu;

            menu.ExitButton = true;

            return menu;
        }

        public static IMenu MenuByType(Type menuType, string title) => menuType switch
        {
            Type t when t == typeof(ChatMenu) => new ChatMenu(title, Instance),
            Type t when t == typeof(CenterHtmlMenu) => new CenterHtmlMenu(title, Instance),
            Type t when t == typeof(WasdMenu) => new WasdMenu(title, Instance),
            Type t when t == typeof(ScreenMenu) => new ScreenMenu(title, Instance),
            _ => new CenterHtmlMenu(title, Instance)
        };

        public static readonly Dictionary<string, Type> MenuTypes = new()
        {
            { "ChatMenu", typeof(ChatMenu) },
            { "CenterHtmlMenu", typeof(CenterHtmlMenu) },
            { "WasdMenu", typeof(WasdMenu) },
            { "ScreenMenu", typeof(ScreenMenu) }
        };
    }
}