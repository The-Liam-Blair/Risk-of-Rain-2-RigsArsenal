using MoreItems.DOTs;
using RoR2;
using UnityEngine;
using static MoreItems.MoreItems;

namespace MoreItems.Buffs
{
    public class RazorLeechWound : Buff
    {
        public override string Name => "RazorLeechWound";
        public override bool canStack => false;
        public override bool isDebuff => true;
        public override Color BuffColor => Color.white;
        public override Sprite Icon => null;


        public override void SetupHooks()
        {
            On.RoR2.HealthComponent.TakeDamage += (orig, self, info) =>
            {
                orig(self, info);

                if (!self) { return; }

                var victim = self.body;
                if (!victim || !victim.inventory) { return; }

                var attackerObj = info.attacker;
                if (!attackerObj || !attackerObj.GetComponent<CharacterBody>()) { return; }

                var attacker = attackerObj.GetComponent<CharacterBody>();
                if (!attacker.inventory) { return; }

                // Ignore damage types that are environmental or dots.
                if(info.damageType is DamageType.DoT or DamageType.NonLethal or DamageType.FallDamage or DamageType.OutOfBounds) { return; }

                DOT leechBleed = DOTList.Find(x => x.Name.Equals("RazorLeechBleed"));

                var roll = 25f; // 25% chance.

                if (victim.HasBuff(buffDef) && Util.CheckRoll(roll, attacker.master))
                {
                    MoreItems.InflictCustomDot(attacker, victim, leechBleed, info.damage * 0.5f);
                }

            };
        }
    }
}
