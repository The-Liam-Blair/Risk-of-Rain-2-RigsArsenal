/*using MoreItems.Buffs;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static MoreItems.MoreItems;


namespace MoreItems.DOTs
{
    public class RazorLeechBleed : DOT
    {
        public override string Name => "RazorLeechBleed";

        public override bool canStack => true;

        public override Color BuffColor => Color.white;

        public override Sprite Icon => null;

        public override float dotDamageCoefficient => 1f;

        public override float dotInterval => 0.2f;

        public override DamageColorIndex dotDamageColorIndex => DamageColorIndex.Bleed;

        public override void Init()
        {
            base.Init();

            dotIndex = DotAPI.RegisterDotDef(dotDef, (self, stack) =>
            {
                if(stack.attackerObject)
                {
                    // Adjusts the number of ticks to 6, over a duration of around 2 to 3 seconds. Total damage is unmodified.
                    stack.damage *= dotDef.interval;
                }
            });


        }
    }
}
*/