using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;

namespace DagonSharp
{
    internal class Program
    {
        private static Hero me;

        private static Item dagon;

        private static bool loaded;

        private static readonly Menu Menu = new Menu("DagonSharp", "DagonSharp", true);

        private static readonly int[] DagonDamage = { 400, 500, 600, 700, 800 };

        private static readonly string[] IgnoreModifiers = {
            "modifier_templar_assassin_refraction_absorb",
            "modifier_item_blade_mail_reflect",
            "modifier_item_lotus_orb_active",
            "modifier_nyx_assassin_spiked_carapace",
            "modifier_medusa_stone_gaze_stone",
            "modifier_winter_wyvern_winters_curse"
        };

        private static void Main()
        {
            Menu.AddItem(new MenuItem("key", "Enabled").SetValue(new KeyBind('K', KeyBindType.Toggle, true)));
            Menu.AddToMainMenu();

            Game.OnUpdate += Game_OnUpdate;
        }


        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsChatOpen || Game.IsWatchingGame || !Utils.SleepCheck("dagonDelay"))
            {
                return;
            }

            if (!loaded)
            {
                me = ObjectManager.LocalHero;

                if (!Game.IsInGame || me == null)
                {
                    return;
                }

                loaded = true;
            }

            if (me.IsChanneling() || me.IsInvisible() || !Menu.Item("key").GetValue<KeyBind>().Active) return;
            dagon = me.GetDagon();
            var target = ObjectManager.GetEntities<Hero>().FirstOrDefault(CheckTarget);

            if (dagon == null || target == null || !me.CanUseItems() || !dagon.CanBeCasted()) return;
            dagon.UseAbility(target);
            Utils.Sleep(100, "dagonDelay");
        }

        private static bool CheckTarget(Unit enemy)
        {
            if (enemy.IsIllusion || !enemy.IsValidTarget(dagon.GetCastRange(), true, me.NetworkPosition))
                return false;

            if (enemy.IsLinkensProtected() || enemy.IsMagicImmune())
                return false;

            if (!enemy.CanDie() || enemy.Modifiers.Any(x => IgnoreModifiers.Any(x.Name.Equals)))
                return false;

            if (me.FindItem("item_aether_lens") != null)
                return enemy.Health <
                       enemy.DamageTaken(DagonDamage[dagon.Level - 1] + DagonDamage[dagon.Level - 1] * ((me.TotalIntelligence / 16 + 5) * 0.01f), DamageType.Magical, me);

            return enemy.Health <
                   enemy.DamageTaken(DagonDamage[dagon.Level - 1] + DagonDamage[dagon.Level - 1] * (me.TotalIntelligence / 16 * 0.01f), DamageType.Magical, me);
        }
    }
}
