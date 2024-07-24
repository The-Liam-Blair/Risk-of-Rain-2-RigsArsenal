using System;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static MoreItems.MoreItems;

namespace MoreItems.Buffs
{
    public class PrimordialChunkAttackSpeedBuff : Buff
    {
        public override string Name => "PrimordialChunkAttackSpeed";
        public override bool canStack => true;
        public override bool isDebuff => false;
        public override Color BuffColor => Color.white;
        public override Sprite Icon => null;

        public override void SetupHooks()
        {
            // For each stack of the buff, give +9% attack speed.
            R2API.RecalculateStatsAPI.GetStatCoefficients += (self, args) =>
            {
                if (!self || !self.inventory || !self.isPlayerControlled) { return; }
                if (!self.HasBuff(buffDef)) { return; }

                args.attackSpeedMultAdd += 0.09f * self.GetBuffCount(buffDef);
            };
        }
    }
}