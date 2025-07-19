using RigsArsenal.DOTs;
using RigsArsenal.Items;
using RigsArsenal.Items.VoidItems;
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

        // Base value of 0.2 (20% of damage dealt).
        private float damageScalar = RazorLeeches.damageScalar.Value;



        public override void SetupHooks()
        {
            GlobalEventManager.onServerDamageDealt += (damageReport) =>
            {
                var victim = damageReport.victim.body;
                if (!victim || !victim.inventory || !victim.gameObject || !victim.healthComponent) { return; }

                var attacker = damageReport.attackerBody;
                if (!attacker || !attacker.inventory || !attacker.gameObject || !attacker.healthComponent) { return; }


                var damageInfo = damageReport.damageInfo;

                // Skip if the victim is not wounded, and ignore damage types that are environmental or dots.
                if (!victim.HasBuff(buffDef)
                    || damageInfo.damageType == DamageType.DoT
                    || damageInfo.damageType == DamageType.NonLethal
                    || damageInfo.damageType == DamageType.FallDamage
                    || damageInfo.damageType == DamageType.OutOfBounds) { return; }

                // Inflict the DOT equal to 20% of the damage dealt.
                leechBleed ??= DOTList.Find(x => x.Name.Equals("RazorLeechBleed"));
                RigsArsenal.InflictCustomDot(attacker, victim, leechBleed, damageInfo.damage * damageScalar);
            };
        }
    }
}