using System.Runtime.CompilerServices;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace MoreItems.Items
{
    /// <summary>
    /// Stimpack - T1 (Common) Item
    /// <para>If the player takes damage while at or under the <paramref name="lowHealthThreshold">the threshold</paramref>, gain a speed boost buff.</para>
    /// <para>Item count increases velocity increase scalar. Duration is infinite while under the threshold.</para>
    /// </summary>
    public class Stimpack : Item
    {

        public override string Name => "Stimpack";
        public override string NameToken => "STIMPACK";
        public override string PickupToken => "Increased movement speed and health regeneration when health is low.";
        public override string Description => "Gain <style=cIsUtility>+10%</style><style=cStack> (+10% per stack)</style> <style=cIsUtility>movement speed</style> and <style=cIsHealth>+1</style> <style=cStack>(+0.5 per stack)</style> <style=cIsHealth>health regeneration</style> while at or under <style=cIsHealth>50% health</style>.";
        public override string Lore => "Man... still stuck in this chicken-shit outfit...";

        public override ItemTier Tier => ItemTier.Tier1;

        public override bool CanRemove => true;

        public override bool AIBlackList => false;

        public override string IconPath => null;
        public override string ModelPath => null;


        private float lowHealthThreshold = 0.5f;

        private float speedScalar = 0.10f;
        private float finalSpeed = 1f;

        public override void SetupHooks()
        {
            // god this looks a mess
            // will revisit when im smarter :)

            // Called when the entity's stats are recalculated, checks the health, and if under the threshold, directly applies the speed buff
            // (Speed buff is a movement multiplier, scaling by items).
            RecalculateStatsAPI.GetStatCoefficients +=
                (body, args) =>
                {
                    if (body.inventory == null) { return; }

                    var count = body.inventory.GetItemCount(itemDef);
                    if (count > 0 && body.healthComponent.combinedHealthFraction <= lowHealthThreshold)
                    {
                        args.moveSpeedMultAdd += finalSpeed;
                        args.baseRegenAdd += 1f + (0.5f * (count - 1)); // +1 health regen for the first item, +0.5 per additional stack.
                    }
                };

            // Checks healing statistics for those with the item, on the basis that if a heal or regeneration increases the player's current health above the threshold
            // in which case the speed buff is removed.
            On.RoR2.HealthComponent.Heal +=
                (orig, self, amount, procChainMask, nonRegen) =>
                {
                    orig(self, amount, procChainMask, nonRegen);

                    var body = self.body;
                    if (body.inventory == null) { return orig(self, amount, procChainMask, nonRegen); }

                    var count = body.inventory.GetItemCount(itemDef);
                    if (count > 0 && body.healthComponent.combinedHealthFraction <= lowHealthThreshold)
                    {
                        finalSpeed = speedScalar * count;
                    }
                    else
                    {
                        finalSpeed = 0f;
                    }
                    body.RecalculateStats();

                    return orig(self, amount, procChainMask, nonRegen);
                };

            // Checks damage report for those with the item, and checks if the damage puts the player under the health threshold.
            // Unsure if this is the best way for doing this, left as it may catch DOT or unusual misc damage, but will be evaluated later on.
            On.RoR2.HealthComponent.TakeDamage +=
                (orig, self, damageInfo) =>
                {
                    orig(self, damageInfo);

                    var body = self.body;
                    if (body.inventory == null) { return; }

                    var count = body.inventory.GetItemCount(itemDef);
                    if (count > 0 && body.healthComponent.combinedHealthFraction <= lowHealthThreshold)
                    {
                        finalSpeed = speedScalar * count;
                    }
                    else
                    {
                        finalSpeed = 0f;
                    }
                    body.RecalculateStats();
                };
        }
    }
}