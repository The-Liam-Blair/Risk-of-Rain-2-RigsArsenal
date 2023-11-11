using BepInEx;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MoreItems
{
    // Dependencies: R2API, LanaugageAPI, ItemAPI
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]

    // Plugin Metadata
    [BepInPlugin(P_GUID, p_Name, p_Version)]

    // Main Plugin Class
    public class MoreItems : BaseUnityPlugin
    {
        // Plugin metadata and version
        public const string P_GUID = P_Author + "." + p_Name;
        public const string P_Author = "RigsInRags";
        public const string p_Name = "MoreItems";
        public const string p_Version = "a0.0.1";


        public void Awake()
        {
            DebugLog.Init(Logger);
        }

        public void CreateNewItem(ItemDef itemDef, ItemDisplayRuleDict displayRules)
        {
            ItemAPI.Add(new CustomItem(itemDef, displayRules));
        }
    }
}
