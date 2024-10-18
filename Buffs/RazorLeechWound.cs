/*
using RigsArsenal.DOTs;
using RoR2;
using UnityEngine;
using static RigsArsenal.RigsArsenal;

namespace RigsArsenal.Buffs
{
    public class RazorLeechWound : Buff
    {
        public override string Name => "RazorLeechWound";
        public override bool canStack => false;
        public override bool isDebuff => true;
        public override Color BuffColor => Color.white;
        public override Sprite Icon => null;

        private DOT leechBleed = null;


        public override void SetupHooks()
        {
            On.RoR2.HealthComponent.TakeDamage += (orig, self, info) =>
            {
                orig(self, info);

                if (!self) { return; }

                var victim = self.body;
                if (!victim || !victim.inventory || !victim.gameObject || !victim.healthComponent) { return; }

                var attackerObj = info.attacker;
                if (!attackerObj || !attackerObj.GetComponent<CharacterBody>()) { return; }

                var attacker = attackerObj.GetComponent<CharacterBody>();
                if (!attacker.healthComponent || !attacker.inventory) { return; }

                if(leechBleed == null)
                {
                    leechBleed = DOTList.Find(x => x.Name.Equals("RazorLeechBleed"));
                }

                // Ignore damage types that are environmental or dots.
                if(info.damageType == DamageType.DoT
                    || info.damageType == DamageType.NonLethal
                    || info.damageType == DamageType.FallDamage
                    || info.damageType == DamageType.OutOfBounds) { return; }

                var roll = 25f; // 25% chance.

                // 25% chance to apply the custom bleed dot, with total damage equal to 50% of the damage dealt.
                if (victim.HasBuff(buffDef) && Util.CheckRoll(roll, attacker.master))
                {
                    RigsArsenal.InflictCustomDot(attacker, victim, leechBleed, info.damage * 0.5f);
                }

            };
        }
    }
}
*/