namespace SharpScreen
{
    using System;
    using System.Collections.Generic;

    using Ensage;

    internal class Program
    {
        #region Static Fields

        private static bool loaded;

        #endregion

        #region Methods

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInGame)
            {
                loaded = false;
                return;
            }
            if (loaded)
            {
                return;
            }
            var list = new Dictionary<string, float>
                           {
                               { "dota_use_particle_fow", 0 }
                           };
            foreach (var data in list)
            {
                var var = Game.GetConsoleVar(data.Key);
                var.RemoveFlags(ConVarFlags.Cheat);
                var.SetValue(data.Value);
            }
            loaded = true;
        }

        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        #endregion
    }
}