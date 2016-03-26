using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.Common.Extensions;
using SharpDX;

namespace OnlyCrit
{
    internal class Program
    {

        #region Defs
        private static readonly Menu Menu = new Menu("OnlyCrit", "crit", true, "npc_dota_hero_phantom_assassin", true);
        private static bool _loaded = false;
        private static Hero me = null;
        private static Hero enemy = null;
        private static float range = 1000;
        private static float atkRange = 128;

        #endregion

        #region Main

        private static void Main()
        {
            Menu.AddItem(new MenuItem("status", "Enabled").SetValue(false));
            Menu.AddItem(new MenuItem("atkRate", "Attack rate").SetValue(new Slider(500, 50, 1080)));
            Menu.AddItem(new MenuItem("keybind", "Key").SetValue(new KeyBind('K', KeyBindType.Press)));
            Menu.AddToMainMenu();
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        #endregion

        #region Game
        private static void Game_OnUpdate(EventArgs args)
        {
            if (!_loaded)
            {
                me = ObjectMgr.LocalHero;
                if (!Game.IsInGame || me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_PhantomAssassin)
                {
                    return;
                }

                _loaded = true;
            }

            if (!Game.IsInGame || me == null)
            {
                _loaded = false;
                return;
            }

            if (Game.IsPaused)
            {
                return;
            }

            if (enemy == null || !enemy.IsValid)
                enemy = GetClosest_Enemy(me, range);


            if (Game.IsKeyDown(Menu.Item("keybind").GetValue<KeyBind>().Key) && !Game.IsChatOpen && me.AttackRange <= atkRange && Utils.SleepCheck("updater"))
            {
                switch (me.NetworkActivity)
                {
                    case NetworkActivity.Attack:
                        repeat();
                        break;
                    case NetworkActivity.Crit:
                        break;
                }
                Utils.Sleep(4 /*+ Menu.Item("atkRate").GetValue<double>() + Game.Ping*/, "updater");
                return;
            }

        }


        #endregion

        #region Methods

        private static void repeat()
        {
            me.Hold();
            me.Attack(Game.MousePosition/*enemy, false*/);
        }

        private static Hero GetClosest_Enemy(Hero me, float range)
        {
            var enemyHeroes =
            ObjectMgr.GetEntities<Hero>()
                .Where(
                    x =>
                        x.Team == me.GetEnemyTeam() && !x.IsIllusion && x.IsAlive && x.IsVisible
                        && x.Distance2D(me.Position) <= range && !x.IsMagicImmune());
            Hero[] closestHero = { null };
            foreach (
                var enemyHero in
                    enemyHeroes.Where(
                        enemyHero =>
                            closestHero[0] == null ||
                            closestHero[0].Distance2D(me.Position) > enemyHero.Distance2D(me.Position)))
            {
                closestHero[0] = enemyHero;
            }
            return closestHero[0];
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (me.NetworkActivity == NetworkActivity.Crit)
                Drawing.DrawText("Crit", new Vector2((int)HUDInfo.ScreenSizeX() / 2 - 100, 100), new Vector2(26, 26), Color.Red, FontFlags.AntiAlias | FontFlags.DropShadow | FontFlags.Outline);
            if (me.NetworkActivity == NetworkActivity.Attack)
                Drawing.DrawText("Attack", new Vector2((int)HUDInfo.ScreenSizeX() / 2 - 100, 100), new Vector2(26, 26), Color.Red, FontFlags.AntiAlias | FontFlags.DropShadow | FontFlags.Outline);

        }
        #endregion
    }
}