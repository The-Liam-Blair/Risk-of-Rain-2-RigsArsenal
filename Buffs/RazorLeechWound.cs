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

                var itemDef = ItemList.Find(x => x.NameToken.Equals("RAZORLEECHES")).itemDef;
                if(!itemDef) { return; }

                DOT leechBleed = DOTList.Find(x => x.Name.Equals("RazorLeechBleed"));

                var count = attacker.inventory.GetItemCount(itemDef);

                var roll = 1 - (1 / (1 + count * 0.25f)); // Hyperbolic scaling approaches 100% proc chance.
                roll *= 100f; // Convert into percentage (The above calculation has a range of 0 to 1).

                if (victim.HasBuff(buffDef) && Util.CheckRoll(roll, attacker.master))
                {
                    MoreItems.InflictCustomDot(attacker, victim, leechBleed, info.damage * 0.5f);
                }

            };
        }
    }
}
