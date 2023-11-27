using System;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MoreItems.Buffs
{
    public class DamagedCapacitorCooldownBuff : Buff
    {
        public override string Name => "DamagedCapacitorCooldown";
        public override bool canStack => false;
        public override bool isDebuff => true;
        public override Color BuffColor => Color.blue;
        public override Sprite Icon => null;
        public override bool isCooldown => true;
    }
}
