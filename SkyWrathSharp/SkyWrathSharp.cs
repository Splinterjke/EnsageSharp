using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.Common.Extensions;
using SharpDX;

namespace SkyWrathSharp
{
    internal class SkyWrathSharp : Variables
    {
        public static void Init()
        {
            Options.MenuInit();
            Game.OnUpdate += ComboUsage;
            Drawing.OnDraw += TargetIndicator;
        }

        private static void ComboUsage(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;
            if (!loaded)
            {
                me = ObjectManager.LocalHero;
                if (!Game.IsInGame || me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Skywrath_Mage)
                {
                    return;
                }

                loaded = true;
                Game.PrintMessage(
                    "<font face='Calibri Bold'><font color='#fff511'>SkyWrathRage is Injected</font> (credits to <font color='#999999'>Splinter)</font>",
                    MessageType.LogMessage);
            }

            if (me == null || !me.IsValid)
            {
                loaded = false;
                me = ObjectManager.LocalHero;
                return;
            }

            target = me.ClosestToMouseTarget(600);

            if (!Game.IsKeyDown(Menu.Item("comboKey").GetValue<KeyBind>().Key) || Game.IsChatOpen) return;

            GetAbilities();

            if (target == null || !target.IsValid || !target.IsVisible || target.IsIllusion || !target.IsAlive ||
                me.IsChanneling() || target.IsInvul() || HasModifiers()) return;

            if (target.IsLinkensProtected())
            {
                PopLinkens(cyclone);
                PopLinkens(force_staff);
                PopLinkens(atos);
                PopLinkens(sheep);
                PopLinkens(orchid);
                PopLinkens(dagon);
                PopLinkens(silence);
            }
            else
            {
                if (!Utils.SleepCheck("combosleep")) return;
                if (soulring != null && soulring.CanBeCasted() && Menu.Item("soulRing").GetValue<bool>())
                    soulring.UseAbility();

                if (!target.UnitState.HasFlag(UnitState.Hexed) && !target.UnitState.HasFlag(UnitState.Stunned))
                    UseItem(sheep, sheep.GetCastRange());

                CastAbility(silence, silence.GetCastRange());
                CastAbility(slow, 1600);
                CastAbility(bolt, bolt.GetCastRange());
                CastUltimate();

                UseItem(atos, atos.GetCastRange(), 140);

                UseItem(orchid, orchid.GetCastRange());
                UseItem(veil, veil.GetCastRange());
                if (!target.HasModifier("modifier_skywrath_mage_ancient_seal") && silence.CanBeCasted() && Menu.Item("abilities").GetValue<AbilityToggler>().IsEnabled(silence.Name))
                    UseItem(ethereal, silence.GetCastRange());
                else UseItem(ethereal, ethereal.GetCastRange());

                UseItem(dagon, dagon.GetCastRange());
                UseItem(shivas, shivas.GetCastRange());

                Moving();
                Utils.Sleep(150, "combosleep");
            }
        }

        private static void GetAbilities()
        {
            if (!Utils.SleepCheck("GetAbilities")) return;
            soulring = me.FindItem("item_soul_ring");
            force_staff = me.FindItem("item_force_staff");
            cyclone = me.FindItem("item_cyclone");
            orchid = me.FindItem("item_orchid");
            sheep = me.FindItem("item_sheepstick");
            veil = me.FindItem("item_veil_of_discord");
            shivas = me.FindItem("item_shivas_guard");
            dagon = me.Inventory.Items.FirstOrDefault(item => item.Name.Contains("item_dagon"));
            atos = me.FindItem("item_rod_of_atos");
            ethereal = me.FindItem("item_ethereal_blade");
            bolt = me.FindSpell("skywrath_mage_arcane_bolt");
            slow = me.FindSpell("skywrath_mage_concussive_shot");
            silence = me.FindSpell("skywrath_mage_ancient_seal");
            mysticflare = me.FindSpell("skywrath_mage_mystic_flare");
            Utils.Sleep(1000, "GetAbilities");
        }

        private static bool HasModifiers()
        {
            if (target.HasModifiers(modifiersNames, false) ||
                (Menu.Item("bladeMail").GetValue<bool>() && target.HasModifier("modifier_item_blade_mail_reflect")) || !Utils.SleepCheck("HasModifiers"))
                return true;
            Utils.Sleep(100, "HasModifiers");
            return false;
        }

        private static void TargetIndicator(EventArgs args)
        {
            if (!Menu.Item("drawTarget").GetValue<bool>())
            {
                if (circle == null) return;
                circle.Dispose();
                circle = null;
                return;
            }
            if (target != null && target.IsValid && !target.IsIllusion && target.IsAlive && target.IsVisible && me.IsAlive)
                DrawTarget();
            else if (circle != null)
            {
                circle.Dispose();
                circle = null;
            }
        }

        private static void DrawTarget()
        {
            heroIcon = Drawing.GetTexture("materials/ensage_ui/miniheroes/skywrath_mage");
            iconSize = new Vector2(HUDInfo.GetHpBarSizeY() * 2);

            if (
                !Drawing.WorldToScreen(
                    target.Position + new Vector3(0, 0, target.HealthBarOffset / 3),
                    out screenPosition))
            {
                return;
            }

            screenPosition += new Vector2(-iconSize.X, 0);
            Drawing.DrawRect(screenPosition, iconSize, heroIcon);

            if (circle == null)
            {
                circle = new ParticleEffect(@"particles\ui_mouseactions\range_finder_tower_aoe.vpcf", target);
                circle.SetControlPoint(2, new Vector3(me.Position.X, me.Position.Y, me.Position.Z));
                circle.SetControlPoint(6, new Vector3(1, 0, 0));
                circle.SetControlPoint(7, new Vector3(target.Position.X, target.Position.Y, target.Position.Z));
            }
            else
            {
                circle.SetControlPoint(2, new Vector3(me.Position.X, me.Position.Y, me.Position.Z));
                circle.SetControlPoint(6, new Vector3(1, 0, 0));
                circle.SetControlPoint(7, new Vector3(target.Position.X, target.Position.Y, target.Position.Z));
            }
        }

        private static void CastAbility(Ability ability, float range)
        {
            if (ability == null || !ability.CanBeCasted() || target.IsMagicImmune() ||
                !(target.NetworkPosition.Distance2D(me) - target.RingRadius <= range) ||
                !Menu.Item("abilities").GetValue<AbilityToggler>().IsEnabled(ability.Name)) return;

            if (ability.IsAbilityBehavior(AbilityBehavior.UnitTarget))
            {
                ability.UseAbility(target);
            }
            if (ability.IsAbilityBehavior(AbilityBehavior.NoTarget))
            {
                ability.UseAbility();
            }
        }

        private static void CastUltimate()
        {
            if (!Utils.SleepCheck("ulti")
                || !Utils.SleepCheck("etherealsleep") || ethereal.CanBeCasted()
                || silence.CanBeCasted()
                || veil.CanBeCasted()
                || orchid.CanBeCasted()
                || mysticflare == null
                || !mysticflare.CanBeCasted()
                || target.MovementSpeed > 280
                || target.HasModifier("modifier_rune_haste")
                || target.IsMagicImmune()
                || !Menu.Item("abilities").GetValue<AbilityToggler>().IsEnabled("skywrath_mage_mystic_flare")
                || (int)(target.Health / target.MaximumHealth * 100) < Menu.Item("noCastUlti").GetValue<Slider>().Value) return;

            if (!target.CanMove() || target.NetworkActivity == NetworkActivity.Idle ||
                target.UnitState.HasFlag(UnitState.Frozen) || target.UnitState.HasFlag(UnitState.Stunned))
                mysticflare.UseAbility(target.NetworkPosition);
            else
                GetPrediction();
        }

        private static void GetPrediction()
        {
            switch (predictionType.GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    if (target.UnitState.HasFlag(UnitState.Hexed))
                    {
                        mysticflare.UseAbility(Prediction.InFront(target, 142));
                        break;
                    }
                    mysticflare.UseAbility(Prediction.InFront(target, 155));
                    break;

                case 1:
                    if (target.UnitState.HasFlag(UnitState.Hexed))
                    {
                        mysticflare.UseAbility(Prediction.PredictedXYZ(target, 210/target.MovementSpeed*1000));
                        break;
                    }
                    mysticflare.UseAbility(Prediction.PredictedXYZ(target, 230/target.MovementSpeed*1000));
                    break;
            }

        }

        private static void UseItem(Item item, float range, int speed = 0)
        {
            if (item == null || !item.CanBeCasted() || target.IsMagicImmune() || target.MovementSpeed < speed ||
                target.HasModifier(item.Name) || !(target.NetworkPosition.Distance2D(me) - target.RingRadius <= range) ||
                !Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled(item.Name))
                return;

            if (item.IsAbilityBehavior(AbilityBehavior.UnitTarget) && !Equals(item, dagon) && !Equals(item, ethereal))
            {
                item.UseAbility(target);
                return;
            }

            if (item.IsAbilityBehavior(AbilityBehavior.Point))
            {
                item.UseAbility(target.NetworkPosition);
                return;
            }

            if (item.IsAbilityBehavior(AbilityBehavior.Immediate))
            {
                item.UseAbility();
                return;
            }

            if (Equals(item, dagon) &&
                ((!silence.CanBeCasted() | !Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled(silence.Name)) ||
                 (!veil.CanBeCasted() | !Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled(veil.Name)))
                /* || ((!ethereal.CanBeCasted() && target.HasModifier("modifier_item_ethereal_blade_slow")) | !Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled(ethereal.Name))*/)
            {
                item.UseAbility(target);
                return;
            }

            if (!Equals(item, ethereal)) return;
            item.UseAbility(target);
            Utils.Sleep(me.NetworkPosition.Distance2D(target.NetworkPosition) * 1.2, "etherealsleep");
        }

        private static void PopLinkens(Ability item)
        {
            if (item == null || !item.CanBeCasted() || !Menu.Item("popLinkensItems").GetValue<AbilityToggler>().IsEnabled(item.Name) || !Utils.SleepCheck("PopLinkens")) return;
            item.UseAbility(target);
            Utils.Sleep(100, "PopLinkens");
        }

        private static void Moving()
        {
            switch (moveMode.GetValue<StringList>().SelectedValue)
            {
                case "Orbwalk":
                    //if (!Utils.SleepCheck("attackDelay"))
                    //{
                    //    me.Move(target.NetworkPosition);
                    //    break;
                    //}
                    Orbwalking.Orbwalk(target);
                    //Utils.Sleep((int)(me.SecondsPerAttack * 1000), "attackDelay");
                    break;
                case "Move to Mouse":
                    me.Move(Game.MousePosition);
                    break;
                case "Nothing":
                    return;
            }
        }
    }
}