using System;
using System.Linq;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common;
using Ensage.Common.Menu;
using SharpDX;

namespace AntiMage_Spyware
{
    internal class Program
    {
        private static Ability blink, ulti;
        private static Item manta, armlet, mjollnir, mom, medallion, solar, dust, bladeMail, abyssal;
        private static readonly Menu Menu = new Menu("Antimage", "Antimage", true, "npc_dota_hero_antimage", true);
        private static Hero me, target;
        private static bool combo;
        static void Main(string[] args)
        {
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            var menu_utama = new Menu("Options", "opsi");
            menu_utama.AddItem(new MenuItem("enable", "enable").SetValue(true));
            menu_utama.AddItem(new MenuItem("Killsteal", "Killsteal").SetValue(true));
            menu_utama.AddItem(new MenuItem("Illusion", "Illusion Auto").SetValue(true));
            menu_utama.AddItem(new MenuItem("Manta", "Auto Manta").SetValue(true));
            Menu.AddSubMenu(menu_utama);
            Menu.AddToMainMenu();
        }

        private static void KS ()
        {
         
        }
        private static float GetDistance2D(Vector3 p1, Vector3 p2)
        {
            return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
        public static void Game_OnUpdate(EventArgs args)
        {
           
            me = ObjectMgr.LocalHero;
            var targets = ObjectMgr.GetEntities<Hero>().Where(enemy => enemy.Team == me.GetEnemyTeam() && !enemy.IsIllusion() && enemy.IsVisible && enemy.IsAlive && enemy.Health > 0).ToList();
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;

            if (me.ClassID != ClassID.CDOTA_Unit_Hero_AntiMage)
                return;

            if (me == null)
                return;
            if (ulti == null)
                ulti = me.Spellbook.Spell4;

            if (Menu.Item("Killsteal").GetValue<bool>())
            {
                double modif = 0;
                switch (ulti.Level)
                {
                    case 1:
                        modif = .6;
                        break;

                    case 2:
                        modif = .85;
                        break;

                    case 3:
                        modif = 1.1;
                        break;
                    default:
                        break;
                }
               
                foreach (var target in targets)
                {
                    double damage = 0;
                    damage = Math.Floor((target.MaximumMana - target.Mana) * modif);
                    double damagefinal = target.DamageTaken((float)damage, DamageType.Magical, me, false);
                    double damageNeeded = target.Health - damagefinal + ulti.GetCastPoint(ulti.Level) * target.HealthRegeneration;
                    if (damageNeeded < 0 && ulti.CanBeCasted() && Utils.SleepCheck("ulti") && GetDistance2D(target.Position, me.Position) < 600)
                    { 
                        ulti.UseAbility(target);
                        Utils.Sleep(150 + Game.Ping, "ulti");
                    }
                }
            }

            if (blink == null)
                blink = me.Spellbook.Spell2;
            
            if (manta == null)
                manta = me.FindItem("item_manta");
            if (abyssal == null)
                abyssal = me.FindItem("item_abyssal_blade");

            if (manta != null && manta.CanBeCasted() && Menu.Item("Manta").GetValue<bool>() && me.IsSilenced())
            {
                manta.UseAbility();
                Utils.Sleep(150 + Game.Ping, "manta");
            }

            var illus = ObjectMgr.GetEntities<Hero>().Where(x => x.IsAlive && x.IsControllable && x.Team == me.Team && x.IsIllusion && x.Modifiers.Any(y => y.Name != "modifier_kill")).ToList();
            
            if (Menu.Item("Illusion").GetValue<bool>())
            {
                foreach (var enemy in targets)
                {
                    if (enemy.Health > 0)
                    {
                        foreach (var illusion in illus)
                        {
                            if (GetDistance2D(enemy.Position, illusion.Position) < 400 && Utils.SleepCheck(illusion.Handle.ToString()))
                            {
                                illusion.Attack(enemy);
                                Utils.Sleep(1000, illusion.Handle.ToString());
                            }
                        }
                    }
                }
            }
            if (combo && Menu.Item("enable").GetValue<bool>())
            {
                target = me.ClosestToMouseTarget(1000);
                   

                if (target != null && target.IsAlive && !target.IsInvul() && !target.IsIllusion)
                {
                    
                    if (me.CanAttack() && me.CanCast())
                         {

                        var linkens = target.Modifiers.Any(x => x.Name == "modifier_item_spheretarget") || target.Inventory.Items.Any(x => x.Name == "item_sphere");
                        

                        Utils.ChainStun(me, 100, null, false);
                        if (blink != null && blink.CanBeCasted() && me.Distance2D(target) > 300 && me.Distance2D(target) <= 1300 && Utils.SleepCheck("blink"))
                        {
                            blink.UseAbility(target.Position);
                            Utils.Sleep(150 + Game.Ping, "blink");
                        }
                        if (manta != null && manta.CanBeCasted() && me.Distance2D(target) < 300 && Utils.SleepCheck("manta"))
                        {
                            manta.UseAbility();
                            Utils.Sleep(150 + Game.Ping, "manta");
                        }

                        if (abyssal != null && abyssal.CanBeCasted() && Utils.SleepCheck("abyssal"))
                        {
                            abyssal.UseAbility(target);
                            Utils.Sleep(400 + Game.Ping, "abyssal");
                        }

                        if (!blink.CanBeCasted() && Utils.SleepCheck("attack2"))
                        {
                            me.Attack(target);
                            Utils.Sleep(Game.Ping + 1000, "attack2");
                        }

                    }
                    else
                    {
                        if (Utils.SleepCheck("attack1"))
                        {
                            me.Attack(target);
                            Utils.Sleep(1000, "attack1");
                        }
                    }
                }
            }
        }



        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (!Game.IsChatOpen)
            {
                if (Game.IsKeyDown(32))
                {
                    combo = true;
                }
                else
                {
                    combo = false;
                }

            }
        }

    }
}
