using RiskOfOptions.Options;
using RiskOfOptions;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RigsArsenal
{
    /// <summary>
    /// Class that initialises the risk of options config if that mod is present and enabled in the current modlist.
    /// </summary>
    internal class RiskOfOptionsCompatibility
    {
        public static bool enabled
        {
            get => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void SetupRiskOfOptionsConfigs()
        {
            ModSettingsManager.SetModDescription("Mod that adds new items and equipment.", RigsArsenal.P_GUID, "Rigs Arsenal");
            ModSettingsManager.SetModIcon(RigsArsenal.MainAssets.LoadAsset<Sprite>("modIcon.png"));

            //ModSettingsManager.AddOption(new CheckBoxOption(RigsArsenal.EnableUmbralPyreVFX));
            ModSettingsManager.AddOption(new CheckBoxOption(RigsArsenal.EnableShotgunMarker));
        }
    }
}