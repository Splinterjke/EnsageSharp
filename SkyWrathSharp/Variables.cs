using System.Collections.Generic;
using Ensage;
using Ensage.Common.Menu;
using SharpDX;

namespace SkyWrathSharp
{
    internal class Variables
    {
        public const string AssemblyName = "SkyWrathSharp";

        public static string heroName;

        public static string[] modifiersNames =
        {
            "modifier_medusa_stone_gaze_stone",
            "modifier_winter_wyvern_winters_curse",
            "modifier_item_lotus_orb_active"
        };

        public static Dictionary<string, Ability> Abilities;

        public static Dictionary<string, bool> abilitiesDictionary = new Dictionary<string, bool>
        {
            {"skywrath_mage_arcane_bolt", true},
            {"skywrath_mage_concussive_shot", true},
            {"skywrath_mage_ancient_seal", true},
            {"skywrath_mage_mystic_flare", true}
        };

        public static Dictionary<string, bool> popLinkensDictionary = new Dictionary<string, bool>
        {
            {"item_rod_of_atos", true},
            {"item_sheepstick", true},
            {"item_force_staff", true},
            {"item_cyclone", true},
            {"item_orchid", true},
            {"item_dagon", true},
            {"skywrath_mage_ancient_seal", true}
        };

        public static Dictionary<string, bool> magicItemsDictionary = new Dictionary<string, bool>
        {
            {"item_rod_of_atos", true},
            {"item_dagon", true},
            {"item_sheepstick", true},
            {"item_orchid", true},
            {"item_veil_of_discord", true},
            {"item_ethereal_blade", true},
            {"item_shivas_guard", true}
        };

        public static Menu Menu;

        public static Menu magicItems;

        public static Menu popLinkensItems;

        public static Menu abilities;

        public static Menu noCastUlti;

        public static MenuItem comboOrder;

        public static MenuItem comboKey;

        public static MenuItem drawTarget;

        public static MenuItem moveMode;

        public static MenuItem noMoveRange;

        public static MenuItem predictionType;

        public static MenuItem soulRing;

        public static MenuItem bladeMail;

        public static bool loaded;

        public static Ability bolt, slow, silence, mysticflare;

        public static Item soulring, force_staff, cyclone, orchid, sheep, veil, shivas, dagon, atos, ethereal;

        public static Hero me, target;

        public static Vector2 iconSize, screenPosition;

        public static DotaTexture heroIcon;

        public static ParticleEffect circle;
    }
}