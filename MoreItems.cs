﻿using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using RigsArsenal.Items;
using R2API;
using RoR2;
using System.Linq;
using System.Reflection;
using RigsArsenal.Buffs;
using RigsArsenal.Equipments;
using UnityEngine;
using R2API.Utils;
using static RoR2.DotController;
using RoR2.ExpansionManagement;
using RigsArsenal.DOTs;
using HarmonyLib;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace RigsArsenal
{
    [BepInPlugin(P_GUID, P_Name, P_Version)]

    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]

    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]
    [BepInDependency(PrefabAPI.PluginGUID)]
    [BepInDependency(DotAPI.PluginGUID)]

    [BepInDependency("com.rune500.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]

    // Main Plugin Class
    public class RigsArsenal : BaseUnityPlugin
    {
        // Plugin metadata and version
        public const string P_GUID = $"{P_Author}.{P_Name}";
        public const string P_Author = "RigsInRags";
        public const string P_Name = "RigsArsenal";
        public const string P_Version = "1.4.0";

        public static AssetBundle MainAssets;

        public static List<Item> ItemList = new List<Item>();
        public static List<Equipment> EquipmentList = new List<Equipment>();
        public static List<Buff> BuffList = new List<Buff>();
        public static List<DOT> DOTList = new List<DOT>();
        

        public static ConfigEntry<bool> EnableShotgunMarker { get; set; }
        public static ConfigEntry<bool> EnableUmbralPyreVFX { get; set; }
        public static List<ConfigEntry<bool>> EnableItems { get; set; }

        public static ConfigFile configFile;


        public void Awake()
        {
            DebugLog.Init(Logger);

            // Load the asset bundle for this mod.
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RigsArsenal.rigsassets"))
            {
                MainAssets = AssetBundle.LoadFromStream(stream);
            }

            configFile = Config;

            ApplyShaders();

            EnableShotgunMarker = Config.Bind("Wrist-Mounted_Shotgun Config", "EnableShotgunMarker", true, "Shows or hides the range indicator for the Wrist-Mounted Shotgun item.");
            EnableUmbralPyreVFX = Config.Bind("Umbral_Pyre Config", "EnableUmbralPyreVFX", true, "Shows or hides the explosion visual effect for the Umbral Pyre item.");

            // Check for Risk of Options, if present setup the Risk of Options configs for this mod.
            if (RiskOfOptionsCompatibility.enabled)
            {
                RiskOfOptionsCompatibility.SetupRiskOfOptionsConfigs();
            }

            // Fetch all the items by type, and load each one (Populate each item's class definition then add to the item list).
            var Items = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(Item)));

            EnableItems = new List<ConfigEntry<bool>>();

            string itemName = "";

            List <Item> voidItems = new List<Item>();

            // For each item...
            foreach (var item in Items)
            {
                // Instantiate the class.
                Item anItem = (Item)System.Activator.CreateInstance(item);

                // For the config display, to remove invalid characters, convert all spaces into underscores and remove apostrophes.
                itemName = anItem.Name.Replace(" ", "_");
                itemName = itemName.Replace("'", "");

                // If the item is not hidden (NoTier), add it to the config file as a way of disabling the item from loading.
                if(anItem.Tier != ItemTier.NoTier)
                {
                    EnableItems.Add(Config.Bind("_Item Selection", itemName, true, "Enables or disables this item from appearing in game."));
                }

                // If the item is enabled in the config or is a hidden item, initialize it for use in game.
                // NOTE: The item tier check >HAS< to be done first as hidden items interrupt the sequence in the config list.
                //       Bless OR operators skipping the second check if the first is true.
                if (anItem.Tier == ItemTier.NoTier || EnableItems[EnableItems.Count - 1].Value)
                {
                    anItem.Init();
                    ItemList.Add(anItem);

                    if (anItem.Tier == ItemTier.VoidTier1 || anItem.Tier == ItemTier.VoidTier2 || anItem.Tier == ItemTier.VoidTier3)
                    {
                        voidItems.Add(anItem);
                    }
                }
            }

            // Initialize the void pairings for void tier items.
            if(voidItems.Count > 0)
            {
                SetupVoidItem(voidItems);
            }


            // Similar to item implementation but for custom buffs.
            var Buffs = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(Buff)));
            foreach (var buff in Buffs)
            {
                Buff aBuff = (Buff)System.Activator.CreateInstance(buff);
                aBuff.Init();
                BuffList.Add(aBuff);
            }


            // Similar to item implementation but for custom equipment.
            var theEquipment = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(Equipment)));
            foreach (var equipment in theEquipment)
            {
                Equipment equip = (Equipment)System.Activator.CreateInstance(equipment);

                itemName = equip.Name.Replace(" ", "_");
                itemName = itemName.Replace("'", "");

                EnableItems.Add(Config.Bind("_Equipment Selection", itemName, true, "Enables or disables this equipment from appearing in game."));

                if(EnableItems[EnableItems.Count - 1].Value)
                {
                    equip.Init();
                    EquipmentList.Add(equip);
                }
            }



            // Similar to item implementation but for custom DOTs.
            var DOTs = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(DOT)));
            foreach (var DOT in DOTs)
            {
                DOT aDot = (DOT)System.Activator.CreateInstance(DOT);
                aDot.Init();
                DOTList.Add(aDot);
            }

            // Initialise Harmony
            var harmony = new Harmony(P_GUID);
            var targetMethod = AccessTools.Method(typeof(RoR2.HealthComponent), "TakeDamage");

            // Dissonant Edge Patch
            var prefix = typeof(DissonantEdge).GetMethod("TakeDamagePatch", BindingFlags.Public | BindingFlags.Static);
            harmony.Patch(targetMethod, prefix: new HarmonyMethod(prefix));

            // Coolant Pack Patch
            prefix = typeof(CoolantPack).GetMethod("TakeDamagePatch", BindingFlags.Public | BindingFlags.Static);
            harmony.Patch(targetMethod, prefix: new HarmonyMethod(prefix));
        }

        //Spawn all items for debugging purposes
        /*
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.F1))
            {
                foreach (var item in ItemList)
                {
                    DEBUG_SpawnItem(item.NameToken);
                }
                foreach (var equip in EquipmentList)
                {
                    DEBUG_SpawnEquipment(equip.NameToken);
                }
            }
        }
        
        
        private void DEBUG_SpawnItem(string itemName)
        {
            var player = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
            var item = ItemList.Find(x => x.NameToken == itemName);

            PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(item.itemDef.itemIndex), player.position, player.forward * 20f * Random.Range(0.1f, 3f));
        }

        private void DEBUG_SpawnEquipment(string equipmentName)
        {
            var player = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;
            var equip = EquipmentList.Find(x => x.NameToken == equipmentName);

            PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(equip.equipmentDef.equipmentIndex), player.position, player.forward * 20f * Random.Range(0.1f, 3f));
        }
        */

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
                        victimObject = victim.gameObject,
                        attackerObject = attacker.gameObject,
                        totalDamage = new float?(damage),
                        dotIndex = dotType,
                        damageMultiplier = 1f
                    };

                    // If user has an igntion tank, upgrade the dot into a stronger burn.
                    if (dotType == DotIndex.Burn)
                    {
                        StrengthenBurnUtils.CheckDotForUpgrade(attacker.inventory, ref burnDot);
                    }

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
                    DebugLog.Log($"Default dot called for dot {dotType}. This should not happen!");
                    DotController.InflictDot(victim.gameObject, attacker.gameObject, dotType, damage, procCoefficent);
                    break;
            }
        }

        
        /// <summary>
        /// Inflict a custom dot on a target. Similar to the standard dot infliction function but uses a custom dot class parameter instead.
        /// </summary>
        public static void InflictCustomDot(CharacterBody attacker, CharacterBody victim, DOT dot, float damage)
        {
            switch(dot.dotName)
            {
                case RigsArsenalDOTs.RazorLeechBleed:
                    InflictDotInfo leechBleed = new InflictDotInfo()
                    {
                        attackerObject = attacker.gameObject,
                        victimObject = victim.gameObject,
                        dotIndex = dot.dotIndex,
                        preUpgradeDotIndex = dot.dotIndex,
                        duration = dot.dotDuration,
                        damageMultiplier = damage / (dot.dotDuration / dot.dotInterval)
                    };
                    DotController.InflictDot(ref leechBleed);
                    break;
            }
        }
        

        /// <summary>
        /// Add expansion definition and set the transformation pair for void items.
        /// </summary>
        private void SetupVoidItem(List<Item> items)
        {
            // Set up the item pair for the void item and its pure item counterpart.
            On.RoR2.Items.ContagiousItemManager.Init += (orig) =>
            {
                foreach (var item in items)
                {
                    item.itemDef.requiredExpansion = ExpansionCatalog.expansionDefs.FirstOrDefault(x => x.nameToken == "DLC1_NAME");

                    ItemDef.Pair voidTransform = new ItemDef.Pair
                    {
                        itemDef1 = item.pureItemDef,
                        itemDef2 = item.itemDef
                    };

                    // Add the item pair to the item relationship list.
                    ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem]
                    = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddToArray(voidTransform);
                }
                orig();
            };
        }
    }
}