using BepInEx;
using MoreItems.Items;
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
    [BepInPlugin(P_GUID, P_Name, P_Version)]

    // Main Plugin Class
    public class MoreItems : BaseUnityPlugin
    {
        // Plugin metadata and version
        public const string P_GUID = $"{P_Author}.{P_Name}";
        public const string P_Author = "RigsInRags";
        public const string P_Name = "MoreItems";
        public const string P_Version = "0.0.3";

        private Stimpack pack;
        private BountyHunterBadge badge;

        public void Awake()
        {
           DebugLog.Init(Logger);

           // Initialise items
            // todo: Once several items are implemented, create a standard method for detecting item classes instead of hardcoded reference per item. (reflection)
            pack = new Stimpack();
            pack.Init();

            badge = new BountyHunterBadge();
            badge.Init();
        }

        
        private void Update()
        {
            // Stimpack
            if (Input.GetKeyDown(KeyCode.F1))
            {
                DebugLog.Log("F1 pressed, spawning stimpack.");
                // Fetch player's transform.
                var player = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(pack.itemDef.itemIndex), player.position, player.forward * 20f);
            }
            else if (Input.GetKeyDown(KeyCode.F2))
            { 
                DebugLog.Log("F2 pressed, spawning bounty hunter's badge.");
                // Fetch player's transform.
                var player = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(badge.itemDef.itemIndex), player.position, player.forward * 20f);
            }
        }
    }
}
