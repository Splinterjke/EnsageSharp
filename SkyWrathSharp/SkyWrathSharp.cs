using System;
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

            Events.OnLoad += OnLoad;
            Events.OnClose += OnClose;
        }

        private static void OnClose(object sender, EventArgs e)
        {
            Game.OnUpdate -= ComboUsage;
            Drawing.OnDraw -= TargetIndicator;
            loaded = false;
            me = null;
            target = null;
        }

        private static void OnLoad(object sender, EventArgs e)
        {
            if (!loaded)
            {
                me = ObjectManager.LocalHero;
                if (!Game.IsInGame || me == null || me.Name != heroName)
                {
                    return;
                }

                loaded = true;
                Game.PrintMessage(
                    "<font face='Calibri Bold'><font color='#fff511'>" + AssemblyName +
                    " is Injected</font> (credits to <font color='#999999'>Splinter)</font>",
                    MessageType.LogMessage);
                GetAbilities();
                Game.OnUpdate += ComboUsage;
                Drawing.OnDraw += TargetIndicator;
            }

            if (me == null || !me.IsValid)
                loaded = false;
        }

        private static void ComboUsage(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame || Game.IsChatOpen)
                return;

            target = me.ClosestToMouseTarget(ClosestToMouseRange.GetValue<Slider>().Value);
            if (Game.IsKeyDown(comboKey.GetValue<KeyBind>().Key))
            {
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

                    Orbwalk();

                    if (target.Distance2D(me.Position) < 700)
                    {
                        if (Utils.SleepCheck("ezkill"))
                        {
                            ezKill = IsEzKillable();
                            Utils.Sleep(5000, "ezkill");
                        }
                    }

                    if (soulring != null && soulring.CanBeCasted() && soulRing.GetValue<bool>())
                        soulring.UseAbility();

                    if (!target.UnitState.HasFlag(UnitState.Hexed) && !target.UnitState.HasFlag(UnitState.Stunned))
                        UseItem(sheep, sheep.GetCastRange());

                    UseBlink();
                    CastAbility(silence, silence.GetCastRange());
                    CastAbility(bolt, bolt.GetCastRange());
                    CastAbility(slow, slow.GetCastRange());

                    UseItem(atos, atos.GetCastRange(), 140);
                    UseItem(medal, medal.GetCastRange());
                    UseItem(orchid, orchid.GetCastRange());
                    UseItem(bloodthorn, bloodthorn.GetCastRange());
                    UseItem(veil, veil.GetCastRange());
                    UseItem(ethereal, ethereal.GetCastRange());

                    UseDagon();

                    CastUltimate();

                    UseItem(shivas, shivas.GetCastRange());

                    Utils.Sleep(150, "combosleep");
                }
            }

            if (Game.IsKeyDown(harassKey.GetValue<KeyBind>().Key))
            {
                GetAbilities();
                if (target == null || !target.IsValid || !target.IsVisible || target.IsIllusion || !target.IsAlive ||
                    me.IsChanneling() || target.IsInvul() || HasModifiers()) return;
                if (!Utils.SleepCheck("harasssleep")) return;
                Orbwalk();
                CastAbility(bolt, bolt.GetCastRange());
                Utils.Sleep(150, "harasssleep");
            }
        }

        private static void GetAbilities()
        {
            if (!Utils.SleepCheck("GetAbilities")) return;
            blink = me.FindItem("item_blink");
            soulring = me.FindItem("item_soul_ring");
            medal = me.FindItem("item_medallion_of_courage");
            bloodthorn = me.FindItem("item_bloodthorn");
            force_staff = me.FindItem("item_force_staff");
            cyclone = me.FindItem("item_cyclone");
            orchid = me.FindItem("item_orchid");
            sheep = me.FindItem("item_sheepstick");
            veil = me.FindItem("item_veil_of_discord");
            shivas = me.FindItem("item_shivas_guard");
            dagon = me.GetDagon();
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
                (bladeMail.GetValue<bool>() && target.HasModifier("modifier_item_blade_mail_reflect")) ||
                !Utils.SleepCheck("HasModifiers"))
                return true;
            Utils.Sleep(100, "HasModifiers");
            return false;
        }

        private static void TargetIndicator(EventArgs args)
        {
            if (!drawTarget.GetValue<bool>())
            {
                if (circle == null) return;
                circle.Dispose();
                circle = null;
                return;
            }
            if (target != null && target.IsValid && !target.IsIllusion && target.IsAlive && target.IsVisible &&
                me.IsAlive)
            {
                DrawTarget();
                DrawEzKill();
            }
            else if (circle != null)
            {
                circle.Dispose();
                circle = null;
            }
        }

        private static void DrawTarget()
        {
            heroIcon = Drawing.GetTexture("materials/ensage_ui/miniheroes/skywrath_mage");
            iconSize = new Vector2(HUDInfo.GetHpBarSizeY()*2);

            if (
                !Drawing.WorldToScreen(target.Position + new Vector3(0, 0, target.HealthBarOffset/3), out screenPosition))
                return;

            screenPosition += new Vector2(-iconSize.X, 0);
            Drawing.DrawRect(screenPosition, iconSize, heroIcon);

            if (circle == null)
            {
                circle = new ParticleEffect(@"particles\ui_mouseactions\range_finder_tower_aoe.vpcf", target);
                circle.SetControlPoint(2, me.Position);
                circle.SetControlPoint(6, new Vector3(1, 0, 0));
                circle.SetControlPoint(7, target.Position);
            }
            else
            {
                circle.SetControlPoint(2, me.Position);
                circle.SetControlPoint(6, new Vector3(1, 0, 0));
                circle.SetControlPoint(7, target.Position);
            }
        }

        private static void DrawEzKill()
        {
            if (!Menu.Item("ezKillCheck").GetValue<bool>() || Game.IsWatchingGame || Game.IsChatOpen) return;
            switch (ezKillStyle.GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    var icoSize = new Vector2((float) (HUDInfo.GetHpBarSizeY()*2));
                    var icoPos = HUDInfo.GetHPbarPosition(target) - new Vector2(21, 5);
                    switch (IsEzKillable())
                    {
                        case true:
                            ezkillIcon = Drawing.GetTexture("materials/ensage_ui/emoticons/bc_emoticon_fire");
                            break;
                        case false:
                            ezkillIcon = null;
                            break;
                    }
                    Drawing.DrawRect(icoPos, icoSize, ezkillIcon);
                    break;

                case 1:
                    var pos = HUDInfo.GetHPbarPosition(target);
                    var size = HUDInfo.GetHpBarSizeY() + 3;
                    var text = string.Empty;
                    var color = new Color();
                    var fontFlags = FontFlags.AntiAlias | FontFlags.Additive;
                    if (Game.IsKeyDown(comboKey.GetValue<KeyBind>().Key))
                    {
                        pos = pos - new Vector2(58, 0);
                        text = "CASTING...";
                        color = Color.Crimson;
                    }
                    else
                    {
                        switch (IsEzKillable())
                        {
                            case true:
                                pos = pos - new Vector2(40, 0);
                                text = "EZKILL";
                                color = Color.Chartreuse;
                                break;
                            case false:
                                pos = pos - new Vector2(63, 0);
                                text = "NON-EZKILL";
                                color = Color.White;
                                break;
                        }
                    }
                    Drawing.DrawText(text, pos, new Vector2(size), color, fontFlags);
                    break;
            }
        }

        private static void CastAbility(Ability ability, float range)
        {
            if (ability == null || !ability.CanBeCasted() || ability.IsInAbilityPhase || target.IsMagicImmune() ||
                !target.IsValidTarget(range, true, me.NetworkPosition) ||
                !Menu.Item("abilities").GetValue<AbilityToggler>().IsEnabled(ability.Name)) return;

            if (ability.IsAbilityBehavior(AbilityBehavior.UnitTarget))
            {
                ability.UseAbility(target);
                return;
            }
            if (ability.IsAbilityBehavior(AbilityBehavior.NoTarget))
            {
                if (Equals(ability, slow))
                {
                    ability.UseAbility();
                    Utils.Sleep(me.NetworkPosition.Distance2D(target.NetworkPosition)/800*1000, "slowsleep");
                    return;
                }
                ability.UseAbility();
            }
        }

        private static void CastUltimate()
        {
            if (mysticflare == null
                || !Menu.Item("abilities").GetValue<AbilityToggler>().IsEnabled(mysticflare.Name)
                || !mysticflare.CanBeCasted()
                || target.IsMagicImmune()
                || !IsFullDebuffed()
                || ezKill
                || target.HasModifier("modifier_rune_haste")
                || target.Health*100/target.MaximumHealth < Menu.Item("noCastUlti").GetValue<Slider>().Value
                || Prediction.StraightTime(target)/1000 < straightTimeCheck.GetValue<Slider>().Value
                || !Utils.SleepCheck("ebsleep")
                || !Utils.SleepCheck("slowsleep"))
                return;

            if (!target.CanMove() ||
                target.UnitState.HasFlag(UnitState.Rooted) ||
                target.UnitState.HasFlag(UnitState.Stunned))
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
                        mysticflare.UseAbility(Prediction.InFront(target, 87));
                        break;
                    }
                    mysticflare.UseAbility(Prediction.InFront(target, 100));
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

        private static void UseDagon()
        {
            if (dagon == null
                || !dagon.CanBeCasted()
                || target.IsMagicImmune()
                || !(target.NetworkPosition.Distance2D(me) - target.RingRadius <= dagon.CastRange)
                || !Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled("item_dagon")
                || !IsFullDebuffed()
                || !Utils.SleepCheck("ebsleep")) return;
            dagon.UseAbility(target);
        }

        private static void UseItem(Item item, float range, int speed = 0)
        {
            if (item == null || !item.CanBeCasted() || target.IsMagicImmune() || target.MovementSpeed < speed ||
                target.HasModifier(item.Name) || !target.IsValidTarget(range, true, me.NetworkPosition) ||
                !Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled(item.Name))
                return;

            if (item.Name.Contains("ethereal") && IsFullDebuffed())
            {
                item.UseAbility(target);
                Utils.Sleep(me.NetworkPosition.Distance2D(target.NetworkPosition) / 1200 * 1000, "ebsleep");
                return;
            }

            if (item.IsAbilityBehavior(AbilityBehavior.UnitTarget) && !item.Name.Contains("item_dagon"))
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
            }
        }

        private static void PopLinkens(Ability item)
        {
            if (item == null || !item.CanBeCasted() ||
                !Menu.Item("popLinkensItems").GetValue<AbilityToggler>().IsEnabled(item.Name) ||
                !Utils.SleepCheck("PopLinkens")) return;
            item.UseAbility(target);
            Utils.Sleep(100, "PopLinkens");
        }

        private static bool IsFullDebuffed()
        {
            if ((atos != null && atos.CanBeCasted() &&
                 Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled(atos.Name) &&
                 !target.HasModifier("modifier_item_rod_of_atos"))
                ||
                (veil != null && veil.CanBeCasted() &&
                 Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled(veil.Name) &&
                 !target.HasModifier("modifier_item_veil_of_discord"))
                ||
                (silence != null && silence.CanBeCasted() &&
                 Menu.Item("abilities").GetValue<AbilityToggler>().IsEnabled(silence.Name) &&
                 !target.HasModifier("modifier_skywrath_mage_ancient_seal"))
                ||
                (orchid != null && orchid.CanBeCasted() &&
                 Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled(orchid.Name) &&
                 !target.HasModifier("modifier_item_orchid_malevolence"))
                ||
                (ethereal != null && ethereal.CanBeCasted() &&
                 Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled(ethereal.Name) &&
                 !target.HasModifier("modifier_item_ethereal_blade_slow"))
                ||
                (bloodthorn != null && bloodthorn.CanBeCasted() &&
                 Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled(bloodthorn.Name) &&
                 !target.HasModifier("modifier_item_bloodthorn"))
                ||
                (slow != null && slow.CanBeCasted() &&
                 Menu.Item("abilities").GetValue<AbilityToggler>().IsEnabled(slow.Name) &&
                 !target.HasModifier("modifier_skywrath_mage_concussive_shot_slow")))
                return false;
            return true;
        }

        private static bool IsEzKillable()
        {
            if (!Menu.Item("ezKillCheck").GetValue<bool>()) return false;
            int totalDamage = 0;
            int plusPerc = 0;
            uint reqMana = 0;

            if (ethereal != null && ethereal.CanBeCasted() &&
                Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled(ethereal.Name))
            {
                totalDamage +=
                    (int)
                        target.SpellDamageTaken((int) (me.TotalIntelligence*2) + 75, DamageType.Magical, me,
                            ethereal.Name);
                plusPerc += 40;
                reqMana += ethereal.ManaCost;
            }

            if (veil != null && veil.CanBeCasted() &&
                Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled(veil.Name))
            {
                plusPerc += 25;
                reqMana += veil.ManaCost;
            }


            if (silence != null && silence.CanBeCasted() &&
                Menu.Item("abilities").GetValue<AbilityToggler>().IsEnabled(silence.Name))
            {
                plusPerc += (int)((silence.Level - 1) * 5 + 30);
                reqMana += silence.ManaCost;
            }


            if (dagon != null && dagon.CanBeCasted()
                /*Menu.Item("magicItems").GetValue<AbilityToggler>().IsEnabled("item_dagon")*/)
            {
                totalDamage +=
                    (int)
                        target.SpellDamageTaken(dagon.GetAbilityData("damage"), DamageType.Magical, me, dagon.Name,
                            minusMagicResistancePerc: plusPerc);
                reqMana += dagon.ManaCost;
            }
                

            if (bolt != null && bolt.CanBeCasted() &&
                Menu.Item("abilities").GetValue<AbilityToggler>().IsEnabled(bolt.Name))
            {
                if (bolt.Level < 4)
                {
                    totalDamage +=
                        (int)
                            target.SpellDamageTaken((bolt.GetAbilityData("bolt_damage") + me.TotalIntelligence * 1.6f) * 1,
                                DamageType.Magical, me, bolt.Name, minusMagicResistancePerc: plusPerc);
                    reqMana += bolt.ManaCost;
                }
                    
                else
                {
                    totalDamage +=
                        (int)
                            target.SpellDamageTaken((bolt.GetAbilityData("bolt_damage") + me.TotalIntelligence * 1.6f) * 2,
                                DamageType.Magical, me, bolt.Name, minusMagicResistancePerc: plusPerc);
                    reqMana += bolt.ManaCost*2;
                }
            }

            if (slow != null && slow.CanBeCasted() &&
                Menu.Item("abilities").GetValue<AbilityToggler>().IsEnabled(slow.Name))
            {
                totalDamage +=
                    (int)
                        target.SpellDamageTaken(slow.GetAbilityData("damage"), DamageType.Magical, me, slow.Name,
                            minusMagicResistancePerc: plusPerc);
                reqMana += slow.ManaCost;
            }
                

            if (me.CanAttack())
                totalDamage += (int) target.DamageTaken(me.DamageAverage*2, DamageType.Physical, me);

            return reqMana < me.Mana && target.Health < totalDamage;
        }

        private static void Orbwalk()
        {
            switch (moveMode.GetValue<bool>())
            {
                case true:
                    Orbwalking.Orbwalk(target);
                    break;
                case false:
                    break;
            }
        }

        private static void UseBlink()
        {
            if (!useBlink.GetValue<bool>() || blink == null || !blink.CanBeCasted() ||
                target.Distance2D(me.Position) < 600 || !Utils.SleepCheck("blink")) return;
            predictXYZ = target.NetworkActivity == NetworkActivity.Move
                ? Prediction.InFront(target,
                    (float) (target.MovementSpeed*(Game.Ping/1000 + 0.3 + target.GetTurnTime(target))))
                : target.Position;

            if (me.Position.Distance2D(predictXYZ) > 1200)
            {
                predictXYZ = (predictXYZ - me.Position)*1200/predictXYZ.Distance2D(me.Position) + me.Position;
            }

            blink.UseAbility(predictXYZ);
            Utils.Sleep(500, "blink");
        }
    }
}