using System;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static MoreItems.MoreItems;

namespace MoreItems.Buffs
{
    public class KineticBatteryCooldownBuff : Buff
    {
        public override string Name => "KineticBatteryCooldown";
        public override bool canStack => false;
        public override bool isDebuff => true;
        public override Color BuffColor => Color.white;
        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("KineticBatteryBuff.png");
        public override bool isCooldown => true;

        // Does not hook into anything, as the buff itself only acts as a timer/cooldown indicator.
    }
}
