using System.Runtime.CompilerServices;
using EntityStates;
using R2API;
using R2API.Utils;
using Rewired;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using static MoreItems.MoreItems;

namespace MoreItems.Items
{
    /// <summary>
    /// Non-usable Item.
    /// <para> Its only purpose is to provide a sprite and description when the Time Warp equipment consumes an item.</para>
    /// <para>This item is just primarily for flavour text, has no functionality, and should be impossible to pick up.</para>
    /// </summary>
    public class TimeWarpConsume : Item
    {
        public override string Name => "ITEM SACRIFICED";
        public override string NameToken => "TIMEWARPCONSUME";
        public override string PickupToken => "";
        public override string Description => "";
        public override string Lore => "";
        public override ItemTier Tier => ItemTier.NoTier;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { };
        public override bool AIBlackList => true;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("TimeWarpConsume.png");
        public override GameObject Model => null;
    }
}