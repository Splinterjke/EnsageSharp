using System;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;

namespace LagHack
{
    internal class LagHack
    {
        private static readonly Menu Menu = new Menu("LagHack", "lh", true);
        private static bool _loaded;
        private static Hero _me;
        private static readonly ConVar CmdRate = Game.GetConsoleVar("cl_cmdrate");

        private static void Main()
        {
            Menu.AddItem(new MenuItem("rate", "Rate").SetValue(new Slider(500, 100, 1000)));
            Menu.AddItem(new MenuItem("keybind", "Key").SetValue(new KeyBind('K', KeyBindType.Press)));
            Menu.AddToMainMenu();
            Game.OnUpdate += Load;
            Game.OnUpdate += Laghack;
        }

        private static void Load(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;
            if (_loaded) return;
            _me = ObjectManager.LocalHero;
            if (!Game.IsInGame || _me == null)
            {
                return;
            }
            Game.PrintMessage(
                "<font face='Calibri Bold'><font color='#fff511'>LagHack is Injected</font> (credits to <font color='#999999'>Splinter)</font>",
                MessageType.LogMessage);
            CmdRate.SetValue(101);
            _loaded = true;
        }

        private static void Laghack(EventArgs args)
        {
            if (!Game.IsKeyDown(Menu.Item("keybind").GetValue<KeyBind>().Key) || Game.IsChatOpen ||
                !Utils.SleepCheck("rate")) return;
            for (var i=0; i<999; i++)
            Game.ExecuteCommand("kill");
            Utils.Sleep(Menu.Item("rate").GetValue<Slider>().Value, "update");
        }
    }
}
