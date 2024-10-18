using System;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static RigsArsenal.RigsArsenal;

namespace RigsArsenal.Buffs
{
    public class SanguineShieldBatteryDuration : Buff
    {
        public override string Name => "SanguineShieldBatteryDuration";
        public override bool canStack => false;
        public override bool isDebuff => false;
        public override Color BuffColor => Color.white;
        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("SanguineShieldBatteryBuff.png");
        public override bool isCooldown => true;

        // Does not hook into anything, as the buff itself only acts as a duration indicator.
    }
}
