using System;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;

namespace OnlyCrit
{
    internal class Program
    {
        private static readonly Menu Menu = new Menu("OnlyCrit", "crit", true);
        private static Hero _me;
        private static Hero _enemy;

        private static void Main()
        {
            Menu.AddItem(new MenuItem("enabled", "Enabled").SetValue(false));
            Menu.AddItem(new MenuItem("atkRate", "Attack rate").SetValue(new Slider(500, 50, 1080)));
            Menu.AddItem(new MenuItem("keybind", "Key").SetValue(new KeyBind('F', KeyBindType.Press)));
            Menu.AddToMainMenu();
            Game.OnUpdate += Game_OnUpdate;
            Game.OnUpdate += GetCrit;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;
            _me = ObjectManager.LocalHero;
            if (_me == null)
                return;
            if (_enemy == null || !_enemy.IsValid)
                _enemy = _me.ClosestToMouseTarget(600);

            if (!Game.IsKeyDown(Menu.Item("keybind").GetValue<KeyBind>().Key) || !Utils.SleepCheck("keybind") || Game.IsChatOpen) return;
            Menu.Item("enabled").SetValue(!Menu.Item("enabled").GetValue<bool>());
            Utils.Sleep(300, "keybind");
        }

        private static void GetCrit(EventArgs args)
        {
            //Game.PrintMessage((_me.BaseAttackTime * 100).ToString(), MessageType.LogMessage);
            if (!Menu.Item("enabled").IsActive() || Game.IsChatOpen || _me.Distance2D(_enemy) > 180 ||
                !_me.IsAttacking() || !Utils.SleepCheck("atkrate")) return;
            if (_me.NetworkActivity == NetworkActivity.Crit) return;
            _me.Hold();
            _me.Attack(_enemy, false);
            Utils.Sleep(_me.BaseAttackTime* 100, "atkrate");

        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_me.NetworkActivity == NetworkActivity.Crit)
                Drawing.DrawText("Crit", new Vector2((int)HUDInfo.ScreenSizeX() / 2 - 100, 100), new Vector2(26, 26),
                    Color.Red, FontFlags.AntiAlias | FontFlags.DropShadow | FontFlags.Outline);
            if (_me.NetworkActivity == NetworkActivity.Attack)
                Drawing.DrawText("Attack", new Vector2((int)HUDInfo.ScreenSizeX() / 2 - 100, 100), new Vector2(26, 26),
                    Color.Red, FontFlags.AntiAlias | FontFlags.DropShadow | FontFlags.Outline);
        }
    }
}