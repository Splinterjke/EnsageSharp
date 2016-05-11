using Ensage.Common.Menu;
using SharpDX;

namespace SkyWrathSharp
{
    internal class Options : Variables
    {
        public static void MenuInit()
        {
            heroName = "npc_dota_hero_skywrath_mage";
            Menu = new Menu(AssemblyName, AssemblyName, true, heroName, true);
            comboKey = new MenuItem("comboKey", "Combo").SetValue(new KeyBind(32, KeyBindType.Press));
            soulRing = new MenuItem("soulRing", "Soulring").SetValue(true).SetTooltip("Use soulring before use the combo if your HP is greater than 150.");
            bladeMail = new MenuItem("bladeMail", "Check for BladeMail").SetValue(false);
            drawTarget = new MenuItem("drawTarget", "Target indicator").SetValue(true);
            moveMode = new MenuItem("moveMode", "Move mode").SetValue(new StringList(new[] { "Orbwalk", "Move to Mouse", "Nothing" }));
            predictionType = new MenuItem("predictionType", "Ultimate prediction").SetValue(new StringList(new[] { "InFront", "By MS/Direction"})).SetTooltip("'InFront' does cast ulti in front of enemy, 'By MS/Direction' calculates movespeed and future target position.");
            noMoveRange = new MenuItem("noMoveRange", "No move/Attack range").SetValue(new Slider(600, 200, 600)).SetTooltip("Range that make the hero stops moving to enemy and starts orbwalking.");
            
            noCastUlti = new Menu("Ultimate usage", "Ultimate usage");
            magicItems = new Menu("Magic Damage Items", "Magic Damage Items");
            popLinkensItems = new Menu("Pop Linkens Items", "Pop Linkens Items");
            abilities = new Menu("Abilities", "Abilities");
            
            Menu.AddItem(comboKey);
            Menu.AddItem(soulRing);
            Menu.AddItem(bladeMail);
            Menu.AddItem(drawTarget);
            Menu.AddItem(moveMode);
            Menu.AddItem(predictionType);
            Menu.AddItem(noMoveRange);

            Menu.AddSubMenu(magicItems);
            Menu.AddSubMenu(popLinkensItems);
            Menu.AddSubMenu(abilities);
            Menu.AddSubMenu(noCastUlti);

            magicItems.AddItem(
                new MenuItem("magicItems", "Magic Damage").SetValue(
                    new AbilityToggler(magicItemsDictionary)));
            popLinkensItems.AddItem(
                new MenuItem("popLinkensItems", "Pop Linken's Items").SetValue(
                    new AbilityToggler(popLinkensDictionary)));
            abilities.AddItem(new MenuItem("abilities", "Abilities").SetValue(
                new AbilityToggler(abilitiesDictionary)));

            noCastUlti.AddItem(
                new MenuItem("noCastUlti", "Do not use ulti if % of enemy's HP is below: ").SetValue(new Slider(35)));

            Menu.AddToMainMenu();
        }

    }
}