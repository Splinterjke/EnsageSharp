using Ensage.Common.Menu;

namespace SkyWrathSharp
{
    internal class Options : Variables
    {
        public static void MenuInit()
        {
            heroName = "npc_dota_hero_skywrath_mage";
            Menu = new Menu(AssemblyName, AssemblyName, true, heroName, true);
            comboKey = new MenuItem("comboKey", "Combo Key").SetValue(new KeyBind(32, KeyBindType.Press));
            soulRing = new MenuItem("soulRing", "Soulring").SetValue(true).SetTooltip("Use soulring before use the combo if your HP is greater than 150.");
            bladeMail = new MenuItem("bladeMail", "Check for BladeMail").SetValue(false);
            drawTarget = new MenuItem("drawTarget", "Target indicator").SetValue(true);
            moveMode = new MenuItem("moveMode", "Orbwalk").SetValue(true);
            predictionType = new MenuItem("predictionType", "Ultimate prediction").SetValue(new StringList(new[] { "InFront", "By MS/Direction"}));
            ezKillCheck = new MenuItem("ezKillCheck", "Check for EZ Kill (coming soon)").SetValue(false).SetTooltip("Check if an enemy is ez-killable (low-mana costs and the fastest way to slay an enemy.");
            straightTimeCheck = new MenuItem("straightTimeCheck", "Straight time before ulti").SetValue(new Slider(0, 0, 2)).SetTooltip("At least time enemy's moving in straight before casting ulti.");
            ClosestToMouseRange = new MenuItem("ClosestToMouseRange", "Closest to mouse range").SetValue(new Slider(600, 500, 1200)).SetTooltip("Range that makes assembly checking for enemy in selected range.");
            
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
            Menu.AddItem(ezKillCheck);
            Menu.AddItem(straightTimeCheck);
            Menu.AddItem(ClosestToMouseRange);

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