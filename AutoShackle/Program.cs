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
        private static Player _player;
        private static Hero _enemy;
        private static float _range;
        private static List<Unit> _creeps = new List<Unit>();
        private static readonly Menu Menu = new Menu("Autoshackle", "autoshackle", true);
        #endregion

        static void Main(string[] args)
        {
            var hotkey = new MenuItem("hotkey", "Toggle hotkey").SetValue(
                new KeyBind('I', KeyBindType.Toggle));
            Menu.AddItem(hotkey);
            Menu.AddItem(new MenuItem("tVector", "Display vector to target:").SetValue(true));
            Menu.AddToMainMenu();
            Game.OnUpdate += Game_OnOnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
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

            if (_enemy == null || !_enemy.IsValid)
            {
                _enemy = GetClosest_Enemy(_me, _range);
            }

            if (_enemy == null || !_enemy.IsValid || !_enemy.IsAlive || !_me.CanCast()) return;

        }


        private static void IngameUpdate(EventArgs args)
        {
            var tree = ObjectManager.GetEntities<Tree>()
                        .Where(x => x.IsAlive && x.IsVisible && x.Distance2D(_me) < 250)
                        .MinOrDefault(x => x.Distance2D(_me));
            Ability quelling_blade;
            quelling_blade = _me.Inventory.Items.FirstOrDefault(x => x.ClassID == ClassID.CDOTA_Item_QuellingBlade);
            Item mango = _me.FindItem("item_tango");
            if (Utils.SleepCheck("tout"))
            {
                //_me.Move(tree.NetworkPosition);
                Game.PrintMessage(tree.Handle.ToString(), MessageType.LogMessage);
                quelling_blade.UseAbility(tree);
                mango.UseAbility(tree);            
                Utils.Sleep(1000, "tout");
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!_loaded) return;
            if (_enemy == null || !_enemy.IsAlive) return;
            draw_vector(_enemy, _me, _range);
            //GetNearestEntities(_enemy, 575);
            //GetNearestUnits(_enemy, 575);
        }

        private static void OnWndProc(EventArgs args)
        {
            //var trees = GetNearestEntities(_me, 500);
            //var tree = GetClosest_Ent(trees, 500, _me);

            //var topor = _me.FindItem("item_quelling_blade");
            //if (Game.IsKeyDown('K') && Utils.SleepCheck("tout"))
            //{
            //    _me.Move(tree.Position);
            //    topor.UseAbility(tree.Position);
            //    Utils.Sleep(200, "tout");

            //}
        }

        private static Hero GetClosest_Enemy(Hero me, float range)
        {
            var enemyHeroes =
            ObjectManager.GetEntities<Hero>()
                .Where(
                    x =>
                        x.Team == me.GetEnemyTeam() && !x.IsIllusion && x.IsAlive && x.IsVisible
                        && x.Distance2D(me.NetworkPosition) <= range && !x.IsMagicImmune());
            Hero[] closestHero = { null };
            foreach (
                var enemyHero in
                    enemyHeroes.Where(
                        enemyHero =>
                            closestHero[0] == null ||
                            closestHero[0].Distance2D(me.NetworkPosition) > enemyHero.Distance2D(me.NetworkPosition)))
            {
                closestHero[0] = enemyHero;
            }
            return closestHero[0];
        }


        private static void draw_vector(Hero enemy, Unit me, float range)
        {
            if (Menu.Item("tVector").GetValue<bool>())
            {
                if (GetDistance(enemy, me) <= Convert.ToDouble(range))
                    Drawing.DrawLine(Drawing.WorldToScreen(me.NetworkPosition), Drawing.WorldToScreen(enemy.NetworkPosition), Color.Green);
            }
        }

        private static Tree GetNearestEntities(Hero me, float range)
        {
            List<Tree> trees = null;
            trees = ObjectManager.GetEntities<Tree>()
                .Where(x => x.Name == "ent_dota_tree" && x.Distance2D(me.NetworkPosition) < range && x.IsAlive)
                .ToList();
            Tree[] _trees = { null };
            foreach (
                var t in
                    trees.Where(
                        t =>
                            _trees[0] == null ||
                            _trees[0].Distance2D(me.NetworkPosition) > t.Distance2D(me.NetworkPosition)))
            {
                _trees[0] = t;
            }
            return _trees[0];
            //foreach (var t in trees)
            //{

            //    //Drawing.DrawLine(Drawing.WorldToScreen(t.Position), Drawing.WorldToScreen(me.Position), Color.Green);
            //}

        }

        private static double GetDistance(Hero target_1, Unit target_2)
        {
            return Math.Sqrt(Math.Pow((target_1.Position.X - target_2.Position.X), 2) + Math.Pow((target_1.Position.Y - target_2.Position.Y), 2)) - (target_1.HullRadius * 2);
        }

        private static void GetNearestUnits(Hero enemy, float range)
        {
            List<Unit> creeps = null;
            creeps = ObjectManager.GetEntities<Unit>()
                .Where(x => x.Distance2D(enemy) <= range && x.Team != _me.Team && !x.IsIllusion && x.IsAlive &&
                            x.IsVisible && !x.IsMagicImmune() && !Equals(x, enemy) && x.ClassID != ClassID.CDOTA_BaseNPC_Tower)
                    .ToList();
            foreach (var c in creeps)
            {
                draw_vector(enemy, c, range);
            }
        }
    }


}

