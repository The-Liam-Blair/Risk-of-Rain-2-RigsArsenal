using System;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MoreItems.Equipments
{
    // Generic item class
    public abstract class Equipment
    {
        public abstract string Name { get; } // Item name
        public abstract string NameToken { get; } // Iten name read by the language API
        public abstract string PickupToken { get; } // Item short description
        public abstract string Description { get; } // Item long description
        public abstract string Lore { get; } // Item lore

        public abstract bool isLunar { get; }
        
        public abstract float cooldown { get; }

        public abstract bool AIBlackList { get; } // Determines if the enemy can receive this item, such as from the void fields.

        public EquipmentDef equipmentDef { get; private set; } // Reference to the item definition.

        public abstract Sprite Icon { get; } // Icon sprite.
        public abstract GameObject Model { get; } // Equipment model.

        public virtual BuffDef EquipmentBuffDef { get; } = null; // Reference to the buff definition.

        public virtual EquipmentSlot EquipmentSlot { get; }

        /// <summary>
        /// Item assembly process: Setup LanguageAPI, create the item object, added to the item list, and reference & implement the methods/events the item hooks into.
        /// </summary>
        public virtual void Init()
        {
            InitLang();
            CreateItem();
            SetupHooks();
        }

        /// <summary>
        /// Setup language API for the item: Name, description and lore.
        /// </summary>
        public virtual void InitLang()
        {
            LanguageAPI.Add($"ITEM_{NameToken}_NAME", Name);
            LanguageAPI.Add($"ITEM_{NameToken}_PICKUP", PickupToken);
            LanguageAPI.Add($"ITEM_{NameToken}_DESCRIPTION", Description);
            LanguageAPI.Add($"ITEM_{NameToken}_LORE", Lore);
        }

        /// <summary>
        /// Generate the item definition, fetch the sprite and model for the item, and add it to the item API.
        /// </summary>
        public virtual void CreateItem()
        {
            equipmentDef = ScriptableObject.CreateInstance<EquipmentDef>();

            equipmentDef.name = $"ITEM_{NameToken}"; // Name has to be XML safe: No spaces or special characters!
            equipmentDef.nameToken = $"ITEM_{NameToken}_NAME";
            equipmentDef.pickupToken = $"ITEM_{NameToken}_PICKUP";
            equipmentDef.descriptionToken = $"ITEM_{NameToken}_DESCRIPTION";
            equipmentDef.loreToken = $"ITEM_{NameToken}_LORE";

            if(isLunar)
            {
                equipmentDef.isLunar = true;
                equipmentDef.colorIndex = ColorCatalog.ColorIndex.LunarItem;
            }
            else
            {
                equipmentDef.isLunar = false;
                equipmentDef.colorIndex = ColorCatalog.ColorIndex.Equipment;
            }


            // If it exists, load custom sprite and model, otherwise load default question mark sprite and model.
            equipmentDef.pickupIconSprite = (Icon != null)
                ? Icon
                : Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();


            equipmentDef.pickupModelPrefab = (Model != null)
                ? Model
                : Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();

            equipmentDef.cooldown = cooldown;

            equipmentDef.appearsInMultiPlayer = true;
            equipmentDef.appearsInSinglePlayer = true;

            equipmentDef.enigmaCompatible = true;
            equipmentDef.canBeRandomlyTriggered = true;

            if(EquipmentBuffDef != null)
            {
                equipmentDef.passiveBuffDef = EquipmentBuffDef;
            }

            ItemAPI.Add(new CustomEquipment(equipmentDef, CreateItemDisplayRules()));

            On.RoR2.EquipmentSlot.PerformEquipmentAction += ActivateEquipment;
        }

        private bool ActivateEquipment(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef _equipDef)
        {
            return (_equipDef == equipmentDef) ? UseEquipment(self) : orig(self, _equipDef);
        }

        public virtual ItemDisplayRuleDict CreateItemDisplayRules() => null;

        public abstract bool UseEquipment(EquipmentSlot slot);
        public virtual void SetupHooks() {}
    }
}