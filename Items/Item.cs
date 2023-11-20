using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MoreItems.Items
{
    // Generic item class
    public abstract class Item
    {
        public abstract string Name { get; }
        public abstract string NameToken { get; }
        public abstract string PickupToken { get; }
        public abstract string Description { get; }
        public abstract string Lore { get; }

        public abstract ItemTier Tier { get; }

        public abstract bool CanRemove { get; }

        public abstract bool AIBlackList { get; }

        public ItemDef itemDef { get; private set; }

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
            LanguageAPI.Add($"ITEM_{NameToken}_NAME{Name}");
            LanguageAPI.Add($"ITEM_{NameToken}_PICKUP{PickupToken}");
            LanguageAPI.Add($"ITEM_{NameToken}_DESCRIPTION{Description}");
            LanguageAPI.Add($"ITEM_{NameToken}_LORE{Lore}");
        }

        /// <summary>
        /// Generate the item definition, fetch the sprite and model for the item, and add it to the item API.
        /// </summary>
        public virtual void CreateItem()
        {
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            itemDef.name = Name; // Name has to be XML safe: No spaces or special characters!
            itemDef.nameToken = NameToken;
            itemDef.pickupToken = PickupToken;
            itemDef.descriptionToken = Description;
            itemDef.loreToken = Lore; // todo- figure out why lore isn't showing in-game.

            switch (itemDef.tier)
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

            //todo: research tags a bit more in-depth and apply to items as per.
            /*
            if (AIBlackList)
            {
                itemDef.tags = new ItemTag[]
                {
                    ItemTag.AIBlacklist
                };
            }
            */

            // Temporary sprites & models
            itemDef.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            itemDef.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();

            ItemAPI.Add(new CustomItem(itemDef, new ItemDisplayRuleDict(null)));
        }

        public virtual void SetupHooks() { }
    }
}