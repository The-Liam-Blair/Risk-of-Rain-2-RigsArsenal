﻿using System;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RigsArsenal.Equipments
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

        public EquipmentDef equipmentDef { get; private set; } // Reference to the item definition.

        public abstract Sprite Icon { get; } // Icon sprite.
        public abstract GameObject Model { get; } // Equipment model.

        // Min and max distances for the equipment's model view in the logbook.
        public abstract float minViewport { get; }
        public abstract float maxViewport { get; }

        public virtual BuffDef EquipmentBuffDef { get; } = null; // Reference to the buff definition.
        public virtual bool IsBuffPassive { get; } = false; // Determines if the buff from the equipment is a constant passive buff or not.

        public virtual EquipmentSlot EquipmentSlot { get; private set; }

        /// <summary>
        /// Item assembly process: Setup LanguageAPI, create the item object, added to the item list, and reference & implement the methods/events the item hooks into.
        /// </summary>
        public virtual void Init()
        {
            AddConfigOptions();
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

            equipmentDef.canDrop = true;

            if(IsBuffPassive)
            {
                equipmentDef.passiveBuffDef = EquipmentBuffDef;
            }


            var modelView = equipmentDef.pickupModelPrefab.AddComponent<ModelPanelParameters>();

            modelView.focusPointTransform = new GameObject("FocusPoint").transform;
            modelView.focusPointTransform.SetParent(equipmentDef.pickupModelPrefab.transform);

            modelView.cameraPositionTransform = new GameObject("CameraPosition").transform;
            modelView.cameraPositionTransform.SetParent(equipmentDef.pickupModelPrefab.transform);

            modelView.modelRotation = equipmentDef.pickupModelPrefab.transform.rotation;

            modelView.minDistance = minViewport;
            modelView.maxDistance = maxViewport;


            ItemAPI.Add(new CustomEquipment(equipmentDef, CreateItemDisplayRules()));

            On.RoR2.EquipmentSlot.PerformEquipmentAction += (orig, self, _equipDef) =>
            {
                if (_equipDef == equipmentDef)
                {
                    return UseEquipment(self);
                }
                return orig(self, _equipDef);
            };

            On.RoR2.EquipmentSlot.UpdateInventory += (orig, self) =>
            {
                orig(self);

                if(self.equipmentIndex == equipmentDef.equipmentIndex)
                {
                    EquipmentSlot = self;
                }
            };
        }

        public virtual ItemDisplayRuleDict CreateItemDisplayRules() => null;

        public abstract bool UseEquipment(EquipmentSlot slot);
        public virtual void SetupHooks() {}
        public virtual void AddConfigOptions() { }

    }
}