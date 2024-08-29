using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MoreItems.Items
{
    // Generic item class
    public abstract class Item
    {
        public abstract string Name { get; } // Item name
        public abstract string NameToken { get; } // Iten name read by the language API
        public abstract string PickupToken { get; } // Item short description
        public abstract string Description { get; } // Item long description
        public abstract string Lore { get; } // Item lore

        public abstract ItemTier Tier { get; } // The tier of the item.

        public virtual ItemTag[] Tags { get; set; }

        public abstract bool CanRemove { get; } // Can be removed from the player's inventory, such as from a shrine of order.
        public abstract bool AIBlackList { get; } // Determines if the enemy can receive this item, such as from the void fields.

        public ItemDef itemDef { get; private set; } // Reference to the item definition.

        public virtual ItemDef pureItemDef { get; set; } = null; // Reference to the "pure" item that corrupts into this item,
                                                                 // if the item is a void item.
                                                                 // For non-void items, it will be null and not used.

        public abstract Sprite Icon { get; } // Icon sprite.
        public abstract GameObject Model { get; } // Item model.

        public virtual BuffDef ItemBuffDef { get; } = null; // Reference to the buff definition.

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
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = $"ITEM_{NameToken}"; // Name has to be XML safe: No spaces or special characters!
            itemDef.nameToken = $"ITEM_{NameToken}_NAME";
            itemDef.pickupToken = $"ITEM_{NameToken}_PICKUP";
            itemDef.descriptionToken = $"ITEM_{NameToken}_DESCRIPTION";
            itemDef.loreToken = $"ITEM_{NameToken}_LORE";

            itemDef.hidden = false;
            itemDef.canRemove = CanRemove;

            switch (Tier)
            {
                case ItemTier.Tier1:
                    itemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
                    break;

                case ItemTier.Tier2:
                    itemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset").WaitForCompletion();
                    break;

                case ItemTier.Tier3:
                    itemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier3Def.asset").WaitForCompletion();
                    break;


                case ItemTier.VoidTier1:
                    itemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/DLC1/Common/VoidTier1Def.asset").WaitForCompletion();
                    break;

                case ItemTier.VoidTier2:
                    itemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/DLC1/Common/VoidTier2Def.asset").WaitForCompletion();
                    break;

                case ItemTier.VoidTier3:
                    itemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/DLC1/Common/VoidTier3Def.asset").WaitForCompletion();
                    break;


                case ItemTier.NoTier:
                    itemDef.tier = ItemTier.NoTier;
                    itemDef.hidden = true;
                    itemDef.deprecatedTier = ItemTier.NoTier;
                    break;


                case ItemTier.Lunar:
                    itemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/LunarTierDef.asset").WaitForCompletion();
                    break;

                default:
                    DebugLog.Log($"Warning: Item {itemDef.name} has an invalid item tier. Defaulting to Tier1.");
                    itemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
                    break;
            }

            // Setup tags list, and special definition for if the item is blacklisted from the enemy AI.
            if (AIBlackList)
            {
                var tagsList = new List<ItemTag>(Tags) { ItemTag.AIBlacklist };
                Tags = tagsList.ToArray();
            }

            if (Tags.Length > 0) { itemDef.tags = Tags; }

            // If it exists, load custom sprite and model, otherwise load default question mark sprite and model.
            itemDef.pickupIconSprite = (Icon != null)
                ? Icon
                : Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();


            itemDef.pickupModelPrefab = (Model != null)
                ? Model
                : Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();


            ItemAPI.Add(new CustomItem(itemDef, CreateItemDisplayRules()));
        }

        public virtual ItemDisplayRuleDict CreateItemDisplayRules() => null;


        public static CharacterModel.RendererInfo[] ItemDisplaySetup(GameObject obj)
        {
            List<Renderer> AllRenderers = new List<Renderer>();

            var meshRenderers = obj.GetComponentsInChildren<MeshRenderer>();
            if (meshRenderers.Length > 0) { AllRenderers.AddRange(meshRenderers); }

            var skinnedMeshRenderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderers.Length > 0) { AllRenderers.AddRange(skinnedMeshRenderers); }

            CharacterModel.RendererInfo[] renderInfos = new CharacterModel.RendererInfo[AllRenderers.Count];

            for (int i = 0; i < AllRenderers.Count; i++)
            {
                renderInfos[i] = new CharacterModel.RendererInfo
                {
                    defaultMaterial = AllRenderers[i] is SkinnedMeshRenderer ? AllRenderers[i].sharedMaterial : AllRenderers[i].material,
                    renderer = AllRenderers[i],
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false //We allow the mesh to be affected by overlays like OnFire or PredatoryInstinctsCritOverlay.
                };
            }

            return renderInfos;
        }

        public virtual void SetupHooks() { }
    }
}