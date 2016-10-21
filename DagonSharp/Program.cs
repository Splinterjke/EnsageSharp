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
        private static Hero me, target;

        private static Item dagon;

        private static bool loaded;

        private static readonly Menu Menu = new Menu("DagonSharp", "DagonSharp", true);

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
            Menu.AddItem(new MenuItem("toggle", "Toggle button").SetValue(new KeyBind('F', KeyBindType.Toggle, true)));
            Menu.AddToMainMenu();

            Game.OnUpdate += Game_OnUpdate;
            Events.OnLoad += OnLoad;
            Events.OnClose += OnClose;
        }

        private static void OnClose(object sender, EventArgs e)
        {
            loaded = false;
            me = null;
            target = null;
        }

        private static void OnLoad(object sender, EventArgs e)
        {
            if (!loaded)
            {
                me = ObjectManager.LocalHero;

                if (!Game.IsInGame || me == null)
                {
                    return;
                }

                loaded = true;
                Game.PrintMessage(
                    "<font face='Calibri Bold'><font color='#fff511'>DagonSharp is Injected</font> (credits to <font color='#999999'>Splinter)</font>",
                    MessageType.LogMessage);
            }

            if (me == null || !me.IsValid)
                loaded = false;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsChatOpen || Game.IsWatchingGame || !Utils.SleepCheck("updaterate"))
                return;

            if (me.IsChanneling() || me.IsInvisible() || !Menu.Item("toggle").GetValue<KeyBind>().Active) return;
            dagon = me.GetDagon();
            target = ObjectManager.GetEntitiesParallel<Hero>().FirstOrDefault(CheckTarget);

            if (dagon == null || target == null || !me.CanUseItems() || !dagon.CanBeCasted()) return;
            dagon.UseAbility(target);
            Utils.Sleep(100, "updaterate");
        }

        private static bool CheckTarget(Unit enemy)
        {
            if (enemy == null || enemy.IsIllusion || !enemy.IsValidTarget(dagon.GetCastRange(), true, me.NetworkPosition) || enemy.IsLinkensProtected() || enemy.IsMagicImmune() || !enemy.CanDie() || enemy.Modifiers.Any(x => IgnoreModifiers.Any(x.Name.Equals)))
                return false;

           return enemy.Health <
                   enemy.SpellDamageTaken(dagon.GetAbilityData("damage"), DamageType.Magical, me, dagon.Name);
        }
    }
}
