using System;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static RigsArsenal.RigsArsenal;

namespace RigsArsenal.Buffs
{
    public class StimpackHealCooldownBuff : Buff
    {
        public override string Name => "StimpackHealCooldown";
        public override bool canStack => false;
        public override bool isDebuff => false;
        public override Color BuffColor => Color.white;
        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("WornOutStimpackBuff.png");

        public override void SetupHooks()
        {
            // While the buff is active, gain +10% movement speed and +0.5 health regen per stack.
            R2API.RecalculateStatsAPI.GetStatCoefficients += (self, args) =>
            {
                var _itemDef = ItemList.Find(x => x.NameToken == "WORNOUTSTIMPACK");
                var itemDef = _itemDef?.itemDef;

                if(!itemDef || !self || !self.inventory) { return; }

                if (!self.HasBuff(buffDef)) { return; }

                var count = self.inventory.GetItemCount(itemDef);

                if (count > 0)
                {
                    args.moveSpeedMultAdd += 0.1f * count;
                    args.baseRegenAdd += 0.5f * count;
                }
            };
        }
    }
}