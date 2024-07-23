using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using MoreItems.Items;
using R2API;
using RoR2;
using System.Linq;
using System.Reflection;
using MoreItems.Buffs;
using MoreItems.Equipments;
using UnityEngine;
using UnityEngine.AddressableAssets;
using R2API.Utils;
using static RoR2.DotController;

namespace MoreItems
{
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]

    [BepInPlugin(P_GUID, P_Name, P_Version)]

    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]

    // Main Plugin Class
    public class MoreItems : BaseUnityPlugin
    {
        // Plugin metadata and version
        public const string P_GUID = $"{P_Author}.{P_Name}";
        public const string P_Author = "RigsInRags";
        public const string P_Name = "RigsArsenal";
        public const string P_Version = "1.1.0";

        public static AssetBundle MainAssets;

        public static List<Item> ItemList = new List<Item>();
        public static List<Equipment> EquipmentList = new List<Equipment>();
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

            ApplyShaders();

            var Buffs = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(Buff)));

           foreach (var buff in Buffs)
           {
               Buff aBuff = (Buff)System.Activator.CreateInstance(buff);
               aBuff.Init();
               BuffList.Add(aBuff);
           }


            // Fetch all the items by type, and load each one (Populate each item's class definition then add to the item list).
            var Items = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(Item)));

            foreach (var item in Items)
            {
                Item anItem = (Item) System.Activator.CreateInstance(item);
                anItem.Init();
                ItemList.Add(anItem);
            }

            var theEquipment = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(Equipment)));

            foreach (var equipment in theEquipment)
            {
                Equipment equip = (Equipment)System.Activator.CreateInstance(equipment);
                equip.Init();
                EquipmentList.Add(equip);
            }

            EnableShotgunMarker = Config.Bind("Wrist-Mounted Shotgun", "EnableShotgunMarker", true, "Shows or hides the range indicator for the wrist-mounted shotgun item.");
        }


        private void Update()
        {
            // Debugging method to spawn items in-game.
            // Disabled for release version.

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
                DebugLog.Log("F7 pressed, spawning Coolant Pack");
                DEBUG_SpawnItem("COOLANTPACK");
            }
            else if (Input.GetKeyDown(KeyCode.F8))
            {
                DebugLog.Log("F8 pressed, spawning Nidus Virus");
                DEBUG_SpawnEquipment("NIDUSVIRUS");
            }
            else if (Input.GetKeyDown(KeyCode.F9))
            {
                DebugLog.Log("F8 pressed, spawning Sanguine Shield Battery");
                DEBUG_SpawnEquipment("SANGUINESHIELDBATTERY");
            }
            else if (Input.GetKeyDown(KeyCode.F10))
            {
                DebugLog.Log("F8 pressed, spawning Time Warp");
                DEBUG_SpawnEquipment("TIMEWARP");
            }

            // Clear all items from the player's inventory.
            else if (Input.GetKeyDown(KeyCode.F11))
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
            else if (Input.GetKeyDown(KeyCode.F10))
            {
                // Give player money
                var player = PlayerCharacterMasterController.instances[0].master;
                player.GiveMoney(100000);
            }
        }

        private void DEBUG_SpawnItem(string itemName)
        {
            var player = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
            var item = ItemList.Find(x => x.NameToken == itemName);

            PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(item.itemDef.itemIndex), player.position, player.forward * 20f);
        }

        private void DEBUG_SpawnEquipment(string equipmentName)
        {
            var player = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
            var equip = EquipmentList.Find(x => x.NameToken == equipmentName);

            PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(equip.equipmentDef.equipmentIndex), player.position, player.forward * 20f);
        }

        /// <summary>
        /// Swap from stubbed shaders to the actual in-game shaders per material (This enables emissions, specular reflections, normal maps, etc).
        /// </summary>
        private static void ApplyShaders()
        {
            var materials = MainAssets.LoadAllAssets<Material>();

            foreach (var mat in materials)
            {
                if (mat.shader.name.StartsWith("StubbedShader"))
                {
                    mat.shader = Resources.Load<Shader>("shaders" + mat.shader.name.Substring(13));
                }
            }   
        }

        /// <summary>
        /// Inflict a standard dot on a target, as how the game applies them.
        /// </summary>
        public static void InflictDot(CharacterBody attacker, CharacterBody victim, DotController.DotIndex dotType, float damage, float procCoefficent = 1f)
        {
            switch (dotType)
            {
                case DotIndex.Bleed:
                    DotController.InflictDot(victim.gameObject, attacker.gameObject, DotIndex.Bleed, 3f * procCoefficent, 1f);
                    break;

                case DotIndex.Burn:
                case DotIndex.StrongerBurn:
                    InflictDotInfo burnDot = new InflictDotInfo()
                    {
                        attackerObject = attacker.gameObject,
                        victimObject = victim.gameObject,
                        totalDamage = damage * 0.5f,
                        damageMultiplier = 1f,
                        dotIndex = DotIndex.Burn
                    };

                    // If user has an igntion tank, upgrade the dot into a stronger burn.
                    StrengthenBurnUtils.CheckDotForUpgrade(attacker.inventory, ref burnDot);

                    DotController.InflictDot(ref burnDot);
                    break;

                case DotIndex.Blight: // Acrid's blight
                    DotController.InflictDot(victim.gameObject, attacker.gameObject, DotIndex.Blight, 5f * procCoefficent, 1f);
                    break;

                case DotIndex.Fracture: // Collapse
                    DotDef collapseDef = GetDotDef(DotIndex.Fracture);
                    DotController.InflictDot(victim.gameObject, attacker.gameObject, DotIndex.Fracture, collapseDef.interval);
                    break;

                case DotIndex.Poison: // Acrid's poison
                    DotController.InflictDot(victim.gameObject, attacker.gameObject, DotIndex.Poison, 5f, 1f);
                    break;

                case DotIndex.SuperBleed: // Bandit's hemorrhage
                    DotController.InflictDot(victim.gameObject, attacker.gameObject, DotIndex.SuperBleed, 15f, 1f);
                    break;

                default: // All the other dots: default implementation
                    DotController.InflictDot(victim.gameObject, attacker.gameObject, dotType, damage, procCoefficent);
                    break;
            }
        }
    }
}
