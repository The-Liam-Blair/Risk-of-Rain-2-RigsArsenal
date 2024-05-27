using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using MoreItems.Items;
using R2API;
using RoR2;
using System.Linq;
using System.Reflection;
using MoreItems.Buffs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using R2API.Utils;

namespace MoreItems
{
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]

    [BepInPlugin(P_GUID, P_Name, P_Version)]

    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]

    // Main Plugin Class
    public class MoreItems : BaseUnityPlugin
    {
        // Plugin metadata and version
        public const string P_GUID = $"{P_Author}.{P_Name}";
        public const string P_Author = "RigsInRags";
        public const string P_Name = "MoreItems";
        public const string P_Version = "0.0.6";

        public static AssetBundle MainAssets;

        public static List<Item> ItemList = new List<Item>();
        public static List<Buff> BuffList = new List<Buff>();
        
        public static ConfigEntry<bool> EnableShotgunMarker { get; set; }


        public void Awake()
        {
            DebugLog.Init(Logger);

            // Load the asset bundle for this mod.
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MoreItems.moreitemsassets"))
           {
                MainAssets = AssetBundle.LoadFromStream(stream);
           }


           var Buffs = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(Buff)));

           foreach (var buff in Buffs)
           {
               DebugLog.Log($"Loading buff {buff.Name}...");
               Buff aBuff = (Buff)System.Activator.CreateInstance(buff);
               aBuff.Init();
               BuffList.Add(aBuff);
               DebugLog.Log($"Buff {buff.Name} loaded.");
           }


            // Fetch all the items by type, and load each one (Populate each item's class definition then add to the item list).
            var Items = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(Item)));

            foreach (var item in Items)
            {
                DebugLog.Log($"Loading item {item.Name}...");
                Item anItem = (Item) System.Activator.CreateInstance(item);
                anItem.Init();
                ItemList.Add(anItem);
                DebugLog.Log($"Item {item.Name} loaded.");
            }

            EnableShotgunMarker = Config.Bind("Wrist-Mounted Shotgun", "EnableShotgunMarker", true, "Shows or hides the range indicator for the wrist-mounted shotgun item.");
        }


        // Debugging method to spawn items in-game.
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                DebugLog.Log("F1 pressed, spawning stimpack.");
                DEBUG_SpawnItem("WORNOUTSTIMPACK");
            }
            else if (Input.GetKeyDown(KeyCode.F2))
            { 
                DebugLog.Log("F2 pressed, spawning bounty hunter's badge.");
                DEBUG_SpawnItem("BOUNTYHUNTERBADGE");
            }

            else if (Input.GetKeyDown(KeyCode.F3))
            {
                DebugLog.Log("F3 pressed, spawning kinetic battery.");
                DEBUG_SpawnItem("KINETICBATTERY");
            }

            else if (Input.GetKeyDown(KeyCode.F4))
            {
                DebugLog.Log("F4 pressed, spawning reactive armour plating.");
                DEBUG_SpawnItem("REACTIVEARMOURPLATING");
            }

            else if (Input.GetKeyDown(KeyCode.F5))
            {
                DebugLog.Log("F5 pressed, spawning Under-Barrel Shotgun");
                DEBUG_SpawnItem("UNDERBARRELSHOTGUN");
            }
            else if (Input.GetKeyDown(KeyCode.F6))
            {
                DebugLog.Log("F6 pressed, spawning Chaos Rune");
                DEBUG_SpawnItem("CHAOSRUNE");
            }
            else if(Input.GetKeyDown(KeyCode.F7))
            {
                DebugLog.Log("F7 pressed, spawning CoolantPack");
                DEBUG_SpawnItem("COOLANTPACK");
            }
            else if (Input.GetKeyDown(KeyCode.F11))
            {
                var player = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
                // apply dot to player
                DotController.InflictDot(player.gameObject, player.gameObject, DotController.DotIndex.Bleed, 5f, 5f);
            }
            else if (Input.GetKeyDown(KeyCode.F12))
            {
                var player = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
                for (int i = 0; i < ItemList.Count; i++)
                {
                    var item = ItemList[i];
                    var count = player.GetComponent<CharacterBody>().inventory.GetItemCount(item.itemDef);
                    if (count > 0)
                    {
                        player.GetComponent<CharacterBody>().inventory.RemoveItem(item.itemDef, count);
                    }
                }
            }
        }

        private void DEBUG_SpawnItem(string itemName)
        {
            var player = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
            var item = ItemList.Find(x => x.NameToken == itemName);

            PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(item.itemDef.itemIndex), player.position, player.forward * 20f);
        }
    }
}
