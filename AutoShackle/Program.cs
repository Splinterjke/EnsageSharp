using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.Common.Extensions;
using SharpDX;


namespace AutoShackle
{
    internal class Program
    {
        #region Members
        private static bool _loaded;
        private static Hero _me;
        private static Hero _enemy;
        private static float _range;
        private static readonly Menu Menu = new Menu("Autoshackle", "autoshackle", true);
        #endregion

        private static void Main()
        {
            var hotkey = new MenuItem("hotkey", "Toggle hotkey").SetValue(
                new KeyBind('I', KeyBindType.Toggle));
            Menu.AddItem(hotkey);
            Menu.AddItem(new MenuItem("tVector", "Display vector to target:").SetValue(true));
            Menu.AddToMainMenu();
            Game.OnUpdate += Game_OnOnUpdate;
            Game.OnIngameUpdate += IngameUpdate;
            _loaded = false;
        }

        private static void Game_OnOnUpdate(EventArgs args)
        {
            _me = ObjectManager.LocalHero;
            _range = 800;
            if (!_loaded)
            {
                if (!Game.IsInGame || _me == null || _me.ClassID != ClassID.CDOTA_Unit_Hero_Windrunner)
                {
                    return;
                }
                _loaded = true;
                Game.PrintMessage("AutoShackle Loaded", MessageType.LogMessage);
            }

            if (!Game.IsInGame || _me == null)
            {
                _loaded = false;
                return;
            }

            if (!_me.IsAlive || Game.IsPaused)
            {
                return;
            }

            if (_enemy == null || !_enemy.IsValid || !_enemy.IsAlive || !_me.CanCast())
            {
                _enemy = _me.ClosestToMouseTarget(_range);
            }

        }


        private static void IngameUpdate(EventArgs args)
        {
            
        }
    }


}

