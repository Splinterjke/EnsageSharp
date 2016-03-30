using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;

namespace SkyWrathRage
{
    internal class Frango
    {
        private static readonly Menu Menu = new Menu("Skywrath rage", "Skywrath rage", true);
        private static readonly Menu MagicItems = new Menu("Magic Damage Items", "Magic Damage Items");
        private static readonly Menu AmplifyItems = new Menu("Magic Amplify Items", "Magic Amplify Items");
        private static readonly Menu DisableItems = new Menu("Disable/slow Items", "Disable/slow Items");
        private static readonly Menu RemoveLinkensItems = new Menu("Pop Linkens Items", "Pop Linkens Items");
        private static readonly Menu Skills = new Menu("Skills", "Skills");
        private static Ability _bolt, _slow, _silence, _mysticflare;
        private static Item _soulring, _force, _cyclone, _orchid, _sheep, _veil, _shivas, _dagon, _atos, _ethereal;
        private static Hero _me, _target;
        private static Vector2 _iconSize, _screenPosition;
        private static DotaTexture _heroIcon;

        private static readonly Dictionary<string, bool> MagicDamage = new Dictionary<string, bool>
        {
            {"item_shivas_guard", true},
            {"item_dagon", true}
        };

        private static readonly Dictionary<string, bool> MagicDamageAmplify = new Dictionary<string, bool>
        {
            {"item_orchid", true},
            {"item_veil_of_discord", true},
            {"item_ethereal_blade", true}
        };

        private static readonly Dictionary<string, bool> DisableSlow = new Dictionary<string, bool>
        {
            {"item_rod_of_atos", true},
            {"item_sheepstick", true},
            {"item_orchid", true},
            {"item_ethereal_blade", true}
        };

        private static readonly Dictionary<string, bool> RemoveLinkens = new Dictionary<string, bool>
        {
            {"item_rod_of_atos", true},
            {"item_sheepstick", true},
            {"skywrath_mage_ancient_seal", true},
            {"skywrath_mage_arcane_bolt", true},
            {"item_force_staff", true},
            {"item_cyclone", true},
            {"item_orchid", true},
            {"item_dagon", true},
            {"item_ethereal_blade", true}
        };

        private static readonly Dictionary<string, bool> SkillsMenu = new Dictionary<string, bool>
        {
            {"skywrath_mage_arcane_bolt", true},
            {"skywrath_mage_concussive_shot", true},
            {"skywrath_mage_ancient_seal", true},
            {"skywrath_mage_mystic_flare", true}
        };

        private static readonly int[] BoltDamage = {60, 80, 100, 120};
        private static ParticleEffect Circle { get; set; }

        private static void Main()
        {
            Menu.AddItem(new MenuItem("Combo Key", "Combo Key").SetValue(new KeyBind('D', KeyBindType.Press)));
            Menu.AddItem(new MenuItem("Chase Key", "Chase Key").SetValue(new KeyBind('T', KeyBindType.Press)));
            Menu.AddItem(
                new MenuItem("Soulring", "Soulring").SetValue(true)
                    .SetTooltip("Use soulring before use the combo if your HP is greater than 150."));
            Menu.AddSubMenu(MagicItems);
            Menu.AddSubMenu(AmplifyItems);
            Menu.AddSubMenu(DisableItems);
            Menu.AddSubMenu(RemoveLinkensItems);
            Menu.AddSubMenu(Skills);
            MagicItems.AddItem(
                new MenuItem("Magic Damage Items", "Magic Damage Items").SetValue(new AbilityToggler(MagicDamage)));
            AmplifyItems.AddItem(
                new MenuItem("Magic Amplify Items", "Magic Amplify Items").SetValue(
                    new AbilityToggler(MagicDamageAmplify)));
            DisableItems.AddItem(
                new MenuItem("Disable/slow Items", "Disable/slow Items").SetValue(new AbilityToggler(DisableSlow)));
            RemoveLinkensItems.AddItem(
                new MenuItem("Pop Linkens Items", "Pop Linkens Items").SetValue(new AbilityToggler(RemoveLinkens)));
            Skills.AddItem(new MenuItem("Skills", "Skills").SetValue(new AbilityToggler(SkillsMenu)));
            Menu.AddToMainMenu();

            // start
            Game.PrintMessage("SkyWrath rage Script Injected!", MessageType.LogMessage);
            Game.OnUpdate += Raging;
            Drawing.OnDraw += Information;
        }

        public static void Raging(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;
            _me = ObjectManager.LocalHero;
            if (_me == null || _me.ClassID != ClassID.CDOTA_Unit_Hero_Skywrath_Mage)
                return;
            _target = _me.ClosestToMouseTarget(600);
            if (Game.IsKeyDown(Menu.Item("Chase Key").GetValue<KeyBind>().Key) && !Game.IsChatOpen)
            {
                FindItems();
                if (_target != null && _target.IsValid && _target.IsVisible && !_target.IsIllusion && _target.IsAlive &&
                    !_me.IsChanneling() && !_target.IsInvul())
                {
                    if (Utils.SleepCheck("FASTCOMBO"))
                    {
                        if (_bolt.Level > 0 && _bolt.CanBeCasted() && !_target.IsMagicImmune() &&
                            Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(_bolt.Name))
                        {
                            _bolt.UseAbility(_target);
                            Utils.Sleep(150, "FASTCOMBO");
                        }
                        else
                            Orbwalking.Orbwalk(_target);
                    }
                }
                else
                {
                    if (!_me.IsChanneling())
                        _me.Move(Game.MousePosition, false);
                }
            }

            if (!Game.IsKeyDown(Menu.Item("Combo Key").GetValue<KeyBind>().Key) || Game.IsChatOpen) return;
            FindItems();
            if (_target != null && _target.IsValid && _target.IsVisible && !_target.IsIllusion && _target.IsAlive &&
                !_me.IsChanneling() && !_target.IsInvul())
            {
                if (_target.IsLinkensProtected())
                {
                    if (!Utils.SleepCheck("DistanceDelay")) return;
                    if (_cyclone != null && _cyclone.CanBeCasted() &&
                        Menu.Item("Pop Linkens Items").GetValue<AbilityToggler>().IsEnabled(_cyclone.Name))
                        _cyclone.UseAbility(_target);
                    else if (_force != null && _force.CanBeCasted() &&
                             Menu.Item("Pop Linkens Items").GetValue<AbilityToggler>().IsEnabled(_force.Name))
                        _force.UseAbility(_target);
                    else if (_orchid != null && _orchid.CanBeCasted() &&
                             Menu.Item("Pop Linkens Items").GetValue<AbilityToggler>().IsEnabled(_orchid.Name))
                        _orchid.UseAbility(_target);
                    else if (_atos != null && _atos.CanBeCasted() &&
                             Menu.Item("Pop Linkens Items").GetValue<AbilityToggler>().IsEnabled(_atos.Name))
                        _atos.UseAbility(_target);
                    else if (_silence.Level >= 1 && _silence.CanBeCasted() &&
                             Menu.Item("Pop Linkens Items").GetValue<AbilityToggler>().IsEnabled(_silence.Name))
                        _silence.UseAbility(_target);
                    else if (_bolt.Level >= 1 && _bolt.CanBeCasted() &&
                             Menu.Item("Pop Linkens Items").GetValue<AbilityToggler>().IsEnabled(_bolt.Name))
                    {
                        _bolt.UseAbility(_target);
                        Utils.Sleep(
                            _me.NetworkPosition.Distance2D(_target.NetworkPosition)/500*1000,
                            "DistanceDelay");
                    }
                    else if (_dagon != null && _dagon.CanBeCasted() &&
                             Menu.Item("Pop Linkens Items").GetValue<AbilityToggler>().IsEnabled("item_dagon"))
                        _dagon.UseAbility(_target);
                    else if (_ethereal != null && _ethereal.CanBeCasted() &&
                             Menu.Item("Pop Linkens Items").GetValue<AbilityToggler>().IsEnabled(_ethereal.Name))
                    {
                        _ethereal.UseAbility(_target);
                        Utils.Sleep(
                            _me.NetworkPosition.Distance2D(_target.NetworkPosition)/
                            1200*1000, "DistanceDelay");
                    }
                    else if (_sheep != null && _sheep.CanBeCasted() &&
                             Menu.Item("Pop Linkens Items").GetValue<AbilityToggler>().IsEnabled(_sheep.Name))
                        _sheep.UseAbility(_target);
                }
                else
                {
                    if (!Utils.SleepCheck("FASTCOMBO")) return;
                    bool ezkillCheck = EZkill(), magicImune = _target.IsMagicImmune();
                    if (_soulring != null && _soulring.CanBeCasted() && Menu.Item("Soulring").GetValue<bool>())
                        _soulring.UseAbility();

                    if (_sheep != null && _sheep.CanBeCasted() && !_target.UnitState.HasFlag(UnitState.Hexed) &&
                        !_target.UnitState.HasFlag(UnitState.Stunned) && !magicImune &&
                        Menu.Item("Disable/slow Items").GetValue<AbilityToggler>().IsEnabled(_sheep.Name))
                        _sheep.UseAbility(_target);

                    if (_silence.Level > 0 && _silence.CanBeCasted() && !magicImune &&
                        Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(_silence.Name))
                        _silence.UseAbility(_target);

                    if (_atos != null && _atos.CanBeCasted() && !magicImune && _target.MovementSpeed >= 200 &&
                        Menu.Item("Disable/slow Items").GetValue<AbilityToggler>().IsEnabled(_atos.Name))
                        _atos.UseAbility(_target);

                    if (_slow.Level > 0 && _slow.CanBeCasted() && _target.NetworkPosition.Distance2D(_me) <= 1600 &&
                        !magicImune && Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(_slow.Name))
                    {
                        _slow.UseAbility();
                        if (Utils.SleepCheck("SlowDelay") && _me.Distance2D(_target) <= _slow.CastRange)
                            Utils.Sleep(_me.NetworkPosition.Distance2D(_target.NetworkPosition)/500*1000,
                                "SlowDelay");
                    }

                    if (_orchid != null && _orchid.CanBeCasted() && !magicImune &&
                        Menu.Item("Magic Amplify Items").GetValue<AbilityToggler>().IsEnabled(_orchid.Name))
                        _orchid.UseAbility(_target);

                    if (_veil != null && _veil.CanBeCasted() && !magicImune &&
                        Menu.Item("Magic Amplify Items").GetValue<AbilityToggler>().IsEnabled(_veil.Name))
                        _veil.UseAbility(_target.NetworkPosition);

                    if (_shivas != null && _shivas.CanBeCasted() && !ezkillCheck && !magicImune &&
                        Menu.Item("Magic Damage Items").GetValue<AbilityToggler>().IsEnabled(_shivas.Name))
                        _shivas.UseAbility();

                    if (_ethereal != null && _ethereal.CanBeCasted() &&
                        (!_veil.CanBeCasted() ||
                         _veil == null |
                         !Menu.Item("Magic Amplify Items").GetValue<AbilityToggler>().IsEnabled(_veil.Name)) &&
                        !magicImune &&
                        Menu.Item("Magic Amplify Items").GetValue<AbilityToggler>().IsEnabled(_ethereal.Name))
                    {
                        _ethereal.UseAbility(_target);
                        if (Utils.SleepCheck("EtherealDelay") && _me.Distance2D(_target) <= _ethereal.CastRange)
                            Utils.Sleep(_me.NetworkPosition.Distance2D(_target.NetworkPosition)/1200*1000,
                                "EtherealDelay");
                    }

                    if (_dagon != null && _dagon.CanBeCasted() &&
                        (!_veil.CanBeCasted() ||
                         _veil == null |
                         !Menu.Item("Magic Amplify Items").GetValue<AbilityToggler>().IsEnabled(_veil.Name)) &&
                        !_silence.CanBeCasted() && !_ethereal.CanBeCasted() && Utils.SleepCheck("EtherealDelay") &&
                        !magicImune &&
                        Menu.Item("Magic Damage Items").GetValue<AbilityToggler>().IsEnabled("item_dagon"))
                        _dagon.UseAbility(_target);

                    if (_bolt.Level > 0 && _bolt.CanBeCasted() && !ezkillCheck && !magicImune &&
                        Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(_bolt.Name))
                        _bolt.UseAbility(_target);

                    if (_mysticflare.Level > 0 && _mysticflare.CanBeCasted() && (Utils.SleepCheck("EtherealDelay")
                                                                                 && Utils.SleepCheck("SlowDelay") ||
                                                                                 _target.UnitState.HasFlag(
                                                                                     UnitState.Frozen) ||
                                                                                 _target.UnitState.HasFlag(
                                                                                     UnitState.Stunned) ||
                                                                                 _target.MovementSpeed <= 280)
                        && !ezkillCheck && Utils.SleepCheck("MysticDamaging")
                        &&
                        (_target.DamageTaken((int) (BoltDamage[_bolt.Level - 1] + _me.TotalIntelligence*1.6),
                            DamageType.Magical, _me)*2 <= _target.Health
                         || _me.Health <= (int) (_me.MaximumHealth*0.35))
                        &&
                        (!_atos.CanBeCasted() ||
                         _atos == null |
                         Menu.Item("Disable/slow Items").GetValue<AbilityToggler>().IsEnabled(_atos.Name)) &&
                        (!_slow.CanBeCasted() ||
                         !Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(_slow.Name)) &&
                        (!_ethereal.CanBeCasted() ||
                         _ethereal == null |
                         !Menu.Item("Magic Amplify Items").GetValue<AbilityToggler>().IsEnabled(_ethereal.Name)) &&
                        !magicImune &&
                        Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled("skywrath_mage_mystic_flare"))
                    {
                        if (!_target.CanMove() || _target.NetworkActivity == NetworkActivity.Idle)
                            _mysticflare.UseAbility(_target.NetworkPosition);
                        else
                            _mysticflare.UseAbility(Prediction.PredictedXYZ(_target, 220/_target.MovementSpeed*1000));
                        var mysticflaredamage = new[] {600, 1000, 1400};
                        if (_target.Health <=
                            _target.DamageTaken(mysticflaredamage[_mysticflare.Level - 1], DamageType.Magical, _me))
                            Utils.Sleep(2500, "MysticDamaging");
                    }
                    Orbwalking.Orbwalk(_target);
                    Utils.Sleep(150, "FASTCOMBO");
                }
            }
            else
            {
                if (!_me.IsChanneling())
                    _me.Move(Game.MousePosition, false);
            }
        }

        private static void FindItems()
        {
            if (!Utils.SleepCheck("FindItems")) return;
            _soulring = _me.FindItem("item_soul_ring");
            _force = _me.FindItem("item_force_staff");
            _cyclone = _me.FindItem("item_cyclone");
            _orchid = _me.FindItem("item_orchid");
            _sheep = _me.FindItem("item_sheepstick");
            _veil = _me.FindItem("item_veil_of_discord");
            _shivas = _me.FindItem("item_shivas_guard");
            _dagon = _me.Inventory.Items.FirstOrDefault(item => item.Name.Contains("item_dagon"));
            _atos = _me.FindItem("item_rod_of_atos");
            _ethereal = _me.FindItem("item_ethereal_blade");
            _bolt = _me.Spellbook.SpellQ;
            _slow = _me.Spellbook.SpellW;
            _silence = _me.Spellbook.SpellE;
            _mysticflare = _me.Spellbook.SpellR;
            Utils.Sleep(300, "FindItems");
        }

        private static bool EZkill()
        {
            if (_target == null || !_target.IsAlive || !_target.IsValid) return false;
            int alldamage = 0, percent = 0;
            var dagondamage = new[] {400, 500, 600, 700, 800};
            var etherealdamage = (int) (_me.TotalIntelligence*2) + 75;
            if (_orchid != null && _orchid.CanBeCasted() &&
                Menu.Item("Magic Amplify Items").GetValue<AbilityToggler>().IsEnabled(_orchid.Name))
                alldamage += 30;
            if (_ethereal != null && _ethereal.CanBeCasted() &&
                Menu.Item("Magic Amplify Items").GetValue<AbilityToggler>().IsEnabled(_ethereal.Name))
                percent += 40;
            if (_veil != null && _veil.CanBeCasted() &&
                Menu.Item("Magic Amplify Items").GetValue<AbilityToggler>().IsEnabled(_veil.Name))
                percent += 25;
            if (_silence.Level > 0 && _silence.CanBeCasted() &&
                Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(_silence.Name))
                percent += (int) ((_silence.Level - 1)*5) + 30;
            if (_dagon != null && _dagon.CanBeCasted() &&
                Menu.Item("Magic Damage Items").GetValue<AbilityToggler>().IsEnabled("item_dagon"))
                alldamage +=
                    (int)
                        _target.DamageTaken(dagondamage[_dagon.Level - 1], DamageType.Magical, _me, false, 0, 0,
                            percent);
            if (_ethereal != null && _ethereal.CanBeCasted() &&
                Menu.Item("Magic Amplify Items").GetValue<AbilityToggler>().IsEnabled(_ethereal.Name))
                alldamage += (int) _target.DamageTaken(etherealdamage, DamageType.Magical, _me, false, 0, 0, 25);
            return _target.Health < alldamage;
        }

        private static void Information(EventArgs args)
        {
            if (_target != null && _target.IsValid && !_target.IsIllusion && _target.IsAlive && _target.IsVisible)
                DrawTarget();
            else if (Circle != null)
                {
                    Circle.Dispose();
                    Circle = null;
                }
        }

        private static void DrawTarget()
        {
            _heroIcon = Drawing.GetTexture("materials/ensage_ui/miniheroes/skywrath_mage");
            _iconSize = new Vector2(HUDInfo.GetHpBarSizeY()*2);

            if (
                !Drawing.WorldToScreen(
                    _target.Position + new Vector3(0, 0, _target.HealthBarOffset/3),
                    out _screenPosition))
            {
                return;
            }

            _screenPosition += new Vector2(-_iconSize.X, 0);
            Drawing.DrawRect(_screenPosition, _iconSize, _heroIcon);

            if (Circle == null)
            {
                Circle = new ParticleEffect(@"particles\ui_mouseactions\range_finder_tower_aoe.vpcf", _target);
                Circle.SetControlPoint(2, new Vector3(_me.Position.X, _me.Position.Y, _me.Position.Z));
                Circle.SetControlPoint(6, new Vector3(1, 0, 0));
                Circle.SetControlPoint(7, new Vector3(_target.Position.X, _target.Position.Y, _target.Position.Z));
            }
            else
            {
                Circle.SetControlPoint(2, new Vector3(_me.Position.X, _me.Position.Y, _me.Position.Z));
                Circle.SetControlPoint(6, new Vector3(1, 0, 0));
                Circle.SetControlPoint(7, new Vector3(_target.Position.X, _target.Position.Y, _target.Position.Z));
            }
        }
    }
}