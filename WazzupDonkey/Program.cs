using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using SharpDX;

namespace WazzupDonkey
{
    internal class Program
    {
        private static readonly Menu Menu = new Menu("What's up Donkey?", "donkey", true);
        private static Player me;
        private static Vector2 screenPos;
        private static List<Courier> Couriers;
        private static List<Courier> coursPos;

        public static void Main()
        {
            Menu.AddItem(new MenuItem("toggler", "Refresh Position")).SetValue(new KeyBind('N', KeyBindType.Press));
            Menu.AddToMainMenu();
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
        }

        public static void Game_OnUpdate(EventArgs args)
        {
            me = ObjectManager.LocalPlayer;
            if (!Game.IsInGame || me == null)
            {
                return;
            }

            Couriers = ObjectManager.GetEntities<Courier>().Where(x => x.Team != me.Team).ToList();
            if (Couriers.Any())
                coursPos = Couriers;

            if (!Utils.SleepCheck("update") || !Game.IsKeyDown(Menu.Item("toggler").GetValue<KeyBind>().Key)) return;
            Game.GetConsoleVar("sv_cheats").RemoveFlags(ConVarFlags.Cheat);
            Game.GetConsoleVar("sv_cheats").SetValue(1);
            Game.ExecuteCommand("cl_fullupdate");
            Utils.Sleep(1100, "update");
        }

        public static void Drawing_OnDraw(EventArgs args)
        {
            if (!Game.IsInGame || me == null || coursPos == null)
            {
                return;
            }

            var heroIcon = Drawing.GetTexture("materials/ensage_ui/other/courier_flying");
            foreach (var ent in coursPos)
            {
                if (!Drawing.WorldToScreen(ent.Position + new Vector3(0, 0, 40), out screenPos))
                    return;
                screenPos += new Vector2(-25, 0);
                Drawing.DrawRect(screenPos, new Vector2(51, 35), heroIcon);
                Drawing.DrawRect(WorldToMiniMap(ent.Position, new Vector2(23, 18)), new Vector2(23, 18), heroIcon);
            }
        }

        public static Vector2 WorldToMiniMap(Vector3 pos, Vector2 size)
        {
            const float MapLeft = -8000;
            const float MapTop = 7350;
            const float MapRight = 7500;
            const float MapBottom = -7200;
            var mapWidth = Math.Abs(MapLeft - MapRight);
            var mapHeight = Math.Abs(MapBottom - MapTop);

            var x = pos.X - MapLeft;
            var y = pos.Y - MapBottom;

            float dx, dy, px, py;
            if (Math.Round((float)Drawing.Width / Drawing.Height, 1) >= 1.7)
            {
                dx = 272f / 1920f * Drawing.Width;
                dy = 261f / 1080f * Drawing.Height;
                px = 11f / 1920f * Drawing.Width;
                py = 11f / 1080f * Drawing.Height;
            }
            else if (Math.Round((float)Drawing.Width / Drawing.Height, 1) >= 1.5)
            {
                dx = 267f / 1680f * Drawing.Width;
                dy = 252f / 1050f * Drawing.Height;
                px = 10f / 1680f * Drawing.Width;
                py = 11f / 1050f * Drawing.Height;
            }
            else
            {
                dx = 255f / 1280f * Drawing.Width;
                dy = 229f / 1024f * Drawing.Height;
                px = 6f / 1280f * Drawing.Width;
                py = 9f / 1024f * Drawing.Height;
            }
            var minimapMapScaleX = dx / mapWidth;
            var minimapMapScaleY = dy / mapHeight;

            var scaledX = Math.Min(Math.Max(x * minimapMapScaleX, 0), dx);
            var scaledY = Math.Min(Math.Max(y * minimapMapScaleY, 0), dy);

            var screenX = px + scaledX;
            var screenY = Drawing.Height - scaledY - py;

            return new Vector2((float)Math.Floor(screenX - size.X / 2), (float)Math.Floor(screenY - size.Y / 2));
        }
    }
}