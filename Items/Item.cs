using System.Collections.Generic;
using R2API;
using RoR2;
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

        public abstract Sprite Icon { get; } // Icon sprite.
        public abstract GameObject Model { get; } // Item model.

        /// <summary>
        /// Item assembly process: Setup LanguageAPI, create the item object, added to the item list, and reference & implement the methods/events the item hooks into.
        /// </summary>
        public virtual void Init()
        {
            DebugLog.Log($"Item {Name} load started...");
            InitLang();

            DebugLog.Log($"Item {Name} language initialised.");
            CreateItem();

            DebugLog.Log($"Item {Name} created.");
            SetupHooks();

            DebugLog.Log($"Item {Name} hooks initialised.");
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

            switch (Tier)
            {
                case ItemTier.Tier1:
                    itemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
                    break;

                case ItemTier.Tier2:
                    itemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset").WaitForCompletion();
                    break;

                //todo: the other tiers

                default:
                    DebugLog.Log($"Item {itemDef.name} has an invalid item tier. Defaulting to Tier1.");
                    itemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
                    break;
            }

            itemDef.canRemove = CanRemove;
            itemDef.hidden = false;

            // Setup tags list, and special definition for if the item is blacklisted from the enemy AI.
            if (AIBlackList)
            {
                Tags = new List<ItemTag>(Tags) { ItemTag.AIBlacklist }.ToArray();
            }

            if(Tags.Length > 0) { itemDef.tags = Tags; }

            // If it exists, load custom sprite and model, otherwise load default question mark sprite and model.
            itemDef.pickupIconSprite = (Icon != null) 
                ? Icon
                : Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();


            itemDef.pickupModelPrefab = (Model != null)
                ? Model
                : Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();

            ItemAPI.Add(new CustomItem(itemDef, new ItemDisplayRuleDict(null)));
        }

        public virtual void SetupHooks() {}
    }
}