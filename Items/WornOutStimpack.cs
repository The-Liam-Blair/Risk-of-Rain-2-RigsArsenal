using System.Runtime.CompilerServices;
using EntityStates;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using static MoreItems.MoreItems;

namespace MoreItems.Items
{
    /// <summary>
    /// WornOutStimpack - T1 (Common) Item
    /// <para>If the player takes damage while at or under 50% health, gain a speed and health regeneration boost.</para>
    /// <para>Item count increases boost intensity. Duration is infinite while under the health threshold.</para>
    /// </summary>
    public class WornOutStimpack : Item
    {

        public override string Name => "Worn-Out Stimpack";
        public override string NameToken => "WORNOUTSTIMPACK";
        public override string PickupToken => "Increased movement speed and health regeneration when health is low.";
        public override string Description => "While at or under <style=cIsHealth>50% health</style>, gain <style=cIsUtility>+10%</style><style=cStack> (+10% per stack)</style> <style=cIsUtility>movement speed</style> and <style=cIsHealing>+0.5 <style=cStack>(+0.5 per stack)</style> base health regeneration</style>.";
        public override string Lore => "<style=cMono>// INTERCEPTED TRANSMISSIONS FROM KOPRULU QUADRANT, SECTOR 19 //</style>\n\n\n''Looks like you've used this here Stimpack one too many times. Save it for when you REALLY need that extra kick.''\n\n''But sir... I need to heal up before the next fight.''\n\n''Nonsense. You will always recover slowly, no matter the situation. But sometimes, you'll fight enemies so strong that the healing isn't enough to offset their damage. That's where the secondary use of your Stimpack becomes clear: the adrenaline, the speed.''\n\n''I don't understand sir.''\n\n''Big strong foes are slow and lumber about. With a speed advantage, you can easily avoid their fire and come out unscathed. Outsmart your foes, and you will always come out victorious.''\n\n\n<style=cMono>// END OF TRANSMISSION //</style>";

        public override ItemTier Tier => ItemTier.Tier1;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] {ItemTag.Healing, ItemTag.Utility};
        public override bool AIBlackList => false;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("WornOutStimpack.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("WornOutStimpack.prefab");


        private float lowHealthThreshold = 0.5f;

        private float speedScalar = 0.10f;
        private float finalSpeed = 0f;

        public override void SetupHooks()
        {
            // god this looks a mess
            // will revisit when im smarter :)

            // Called when the entity's stats are recalculated, checks the health, and if under the threshold, applies the speed and health regen buff.
            RecalculateStatsAPI.GetStatCoefficients +=
                (body, args) =>
                {
                    if (body.inventory == null) { return; }

                    var count = body.inventory.GetItemCount(itemDef);
                    if (count > 0 && body.healthComponent.combinedHealthFraction <= lowHealthThreshold)
                    {
                        args.moveSpeedMultAdd += finalSpeed;
                        args.baseRegenAdd += 0.5f * count;
                    }
                };

            // Checks healing statistics for those with the item, on the basis that if a heal increases the entity's current health above the threshold
            // in which case the buffs are removed.
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

            // Checks damage report for those with the item, and checks if the damage puts the entity under the health threshold.
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