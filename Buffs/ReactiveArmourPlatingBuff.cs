using System;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static MoreItems.MoreItems;

namespace MoreItems.Buffs
{
    public class ReactiveArmourPlatingBuff : Buff
    {
        public override string Name => "ReactiveArmourPlatingBuff";
        public override bool canStack => false;
        public override bool isDebuff => false;
        public override Color BuffColor => Color.white;
        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("ReactiveArmourPlatingBuff.png");

        public override void SetupHooks()
        {
            // While the buff is active, gain +15% total armour per stack.
            R2API.RecalculateStatsAPI.GetStatCoefficients += (self, args) =>
            {
                var itemDef = ItemList.Find(x => x.NameToken == "REACTIVEARMOURPLATING").itemDef;
                var count = self.inventory.GetItemCount(itemDef);

                if (self.HasBuff(buffDef))
                {
                    args.armorAdd += 20 * count;
                }
            };
        }
    }
}