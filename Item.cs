using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MoreItems
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

        public ItemDef ItemDef { get; private set; }

        /// <summary>
        /// Item assembly process: Setup LanguageAPI, create the item object, added to the item list, and reference the methods/events the item hooks into.
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
            LanguageAPI.Add($"Item {NameToken} {Name}");
            LanguageAPI.Add($"Item {NameToken} {PickupToken}");
            LanguageAPI.Add($"Item {NameToken} {Description}");
            LanguageAPI.Add($"Item {NameToken} {Lore}");
        }

        /// <summary>
        /// Generate the item definition, fetch the sprite and model for the item, and add it to the item API.
        /// </summary>
        public virtual void CreateItem()
        {
            ItemDef = ScriptableObject.CreateInstance<ItemDef>();

            ItemDef.name = Name;
            ItemDef.nameToken = NameToken;
            ItemDef.pickupToken = PickupToken;
            ItemDef.descriptionToken = Description;
            ItemDef.loreToken = Lore;

            ItemDef.tier = Tier;

            ItemDef.canRemove = CanRemove;

            // Temporary sprites & models
            ItemDef.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
            ItemDef.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mystery/PickupMystery.prefab").WaitForCompletion();

            ItemAPI.Add(new CustomItem(ItemDef, new ItemDisplayRuleDict()));
        }

        public virtual void SetupHooks() {}
    }
}
