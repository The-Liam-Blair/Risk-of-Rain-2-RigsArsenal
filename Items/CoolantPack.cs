using System.Runtime.CompilerServices;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Diagnostics;
using UnityEngine.UIElements;
using static MoreItems.MoreItems;

namespace MoreItems.Items
{
    /// <summary>
    /// Coolant Pack - T2 (Uncommon) Item
    /// <para>Incoming damaging debuffs deal reduced damage.</para>
    /// <para>Hyperbolic scaling so that 100% damage reduction cannot be reached.</para>
    /// </summary>
    public class CoolantPack : Item
    {
        public override string Name => "Coolant Pack";
        public override string NameToken => "COOLANTPACK";
        public override string PickupToken => "Incoming damaging debuffs inflict less damage";
        public override string Description => "All incoming <style=cIsDamage>damaging debuffs</style> inflict 15% <style=cStack>(+15% per stack)</style> <style=cIsDamage>reduced damage</style>.";
        public override string Lore => "Introducing MediFreeze(tm)! Your omni-purpose solution to all burns, aches and ailments.\n\nCut your finger? Seal that wound up with MediFreeze!\nBurned Yourself? MediFreeze it away!\nBruised your head? MediFreeze it into oblivion!\n\nMediFreeze, buy now today at your nearest wholesaler.\n\nLEGAL DISCLAIMER: Medical trials pending. May contain toxic materials hazardous to biological life. May contain corrosive materials. Wear protective equipment before handling. Seek medical attention immediately if it comes into direct contact with skin, eyes or other sensitive areas.";

        public override ItemTier Tier => ItemTier.Tier2;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Utility };
        public override bool AIBlackList => true;

        public override Sprite Icon => null;
        public override GameObject Model => null;

        public override void SetupHooks()
        {
            On.RoR2.HealthComponent.TakeDamage += (orig, self, info) =>
            {
                var count = self.body.inventory.GetItemCount(itemDef);
                var damageType = info.damageType;
                if(damageType == DamageType.DoT && count > 0)
                {
                    var fractionalBit = 1 - (1 / (1 + count * 0.15f)); // Hyperbolic Scaling, approaching 100% damage reduction.
                    info.damage *= 1 - fractionalBit;
                }

                orig(self, info);
            };

        }
    }
}