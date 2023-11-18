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
        public const string P_GUID = $"{P_Author}.{p_Name}";
        public const string P_Author = "RigsInRags";
        public const string p_Name = "MoreItems";
        public const string p_Version = "0.0.1";

        private Stimpack pack;

        public void Awake()
        {
           DebugLog.Init(Logger);

           // Initialise items
            // todo: Once several items are implemented, create a standard method for detecting item classes instead of hardcoded reference per item. (reflection)
            pack = new Stimpack();
            pack.Init();
        }

        
        private void Update()
        {
            // Stimpack
            if (Input.GetKeyDown(KeyCode.F1))
            {
                DebugLog.Log("F1 pressed, spawning stimpack.");
                // Fetch player's transform.
                var player = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                DebugLog.Log($"Player transform fetched: {player.position}");

                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(pack.itemDef.itemIndex), player.position, player.forward * 20f);
            }
        }
    }
}
