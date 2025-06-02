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
            GlobalEventManager.onServerDamageDealt += (damageReport) =>
            {
                var victim = damageReport.victim.body;
                if (!victim || !victim.inventory || !victim.gameObject || !victim.healthComponent) { return; }

                var attacker = damageReport.attackerBody;
                if (!attacker || !attacker.inventory || !attacker.gameObject || !attacker.healthComponent) { return; }


                if (leechBleed == null) { leechBleed = DOTList.Find(x => x.Name.Equals("RazorLeechBleed")); }

                // Ignore damage types that are environmental or dots.
                var damageInfo = damageReport.damageInfo;

                // Ignore damage types that are environmental or dots.
                if (damageInfo.damageType == DamageType.DoT
                    || damageInfo.damageType == DamageType.NonLethal
                    || damageInfo.damageType == DamageType.FallDamage
                    || damageInfo.damageType == DamageType.OutOfBounds) { return; }

                // Inflict the DOT equal to 20% of the damage dealt.
                if (victim.HasBuff(buffDef))
                {
                    RigsArsenal.InflictCustomDot(attacker, victim, leechBleed, damageInfo.damage * 0.2f);
                }
            };
        }
    }
}