using System.Collections.Generic;
using BepInEx;
using MoreItems.Items;
using R2API;
using RoR2;
using System.Linq;
using System.Reflection;
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
        public const string P_Version = "0.0.4";

        public static AssetBundle MainAssets;

        public static List<Item> ItemList;


        public void Awake()
        {
            // Start up the logger.
            DebugLog.Init(Logger);

            // Load the asset bundle for this mod.
           using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MoreItems.my_assetbundlefile"))
            {
                MainAssets = AssetBundle.LoadFromStream(stream);
            }

           // Fetch all the items by type, and load each one (Populate each item's class definition then add to the item list).
            var Items = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(Item)));

            foreach (var item in Items)
            {
                Item anItem = (Item) System.Activator.CreateInstance(item);
                anItem.Init();
                ItemList.Add(anItem);
            }

        }


        private void Update()
        {
            // Stimpack
            if (Input.GetKeyDown(KeyCode.F1))
            {
                DebugLog.Log("F1 pressed, spawning stimpack.");
                // Fetch player's transform.
                var player = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                var pack = ItemList.Find(x => x.Name == "Stimpack");
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(pack.itemDef.itemIndex), player.position, player.forward * 20f);
            }
            else if (Input.GetKeyDown(KeyCode.F2))
            { 
                DebugLog.Log("F2 pressed, spawning bounty hunter's badge.");
                // Fetch player's transform.
                var player = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                var badge = ItemList.Find(x => x.Name == "BountyHunterBadge");
                PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(badge.itemDef.itemIndex), player.position, player.forward * 20f);
            }
        }
    }
}
