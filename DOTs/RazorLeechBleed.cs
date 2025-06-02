using RigsArsenal.Buffs;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static RigsArsenal.RigsArsenal;


namespace RigsArsenal.DOTs
{
    public class RazorLeechBleed : DOT
    {
        public override string Name => "RazorLeechBleed";

        public override bool canStack => true;

        public override Color BuffColor => Color.white;

        public override Sprite Icon => null;

        public override float dotDamageCoefficient => 1f;

        public override float dotInterval => 0.25f;

        public override float dotDuration => 3f;

        public override RigsArsenalDOTs dotName => RigsArsenalDOTs.RazorLeechBleed;

        public override DamageColorIndex dotDamageColorIndex => DamageColorIndex.Void;

        public override void Init()
        {
            base.Init();

            // Adds custom DOT definition, called after AddDot() in RoR2.DotController.cs
            dotIndex = DotAPI.RegisterDotDef(dotDef, (self, stack) =>
            {
                TeamComponent attacker = stack.attackerObject.GetComponent<TeamComponent>();
                CharacterBody attackerBody = attacker.GetComponent<CharacterBody>();

                if (attackerBody)
                {
                    // Calculate the total and per-tick damage, passed through the stack.damage variable.
                    float totalDamage = stack.damage;
                    float tickDamage = stack.damage / (dotDuration / dotInterval);

                    // Correct the damage to be per tick, and adjust the total DOT duration back to its intended value as it gets overriden in 
                    // RoR2.DotController.AddDot().
                    stack.damage = tickDamage;
                    stack.timer = dotDuration;
                    stack.totalDuration = dotDuration;

                    // Because of the fixed duration, if the totalDamage is < 12 it will still tick 1 damage per second (Minimum clamped damage)
                    // 4 times a second for 3 seconds, even if the DOT's calculated total damage is less than 12.
                    // This quick check handles this edge case to scale the total duration down proportionally to the total damage. (Interval remains the same).
                    if(totalDamage < 12f)
                    {
                        stack.totalDuration = Mathf.Max(dotInterval, dotInterval * totalDamage);
                        stack.timer = stack.totalDuration;
                    }
                }
            });
        }
    }
}