using System.Runtime.CompilerServices;
using EntityStates;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using BepInEx.Configuration;
using static RigsArsenal.RigsArsenal;

namespace RigsArsenal.Items
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
        public override string PickupToken => "Increased movement speed and health regeneration. Effects double at low health.";
        public override string Description => $"Gain <style=cIsUtility>{movementBonus.Value*100}%</style><style=cStack> (+{movementBonus.Value*100}% per stack)</style> <style=cIsUtility>movement speed</style> and <style=cIsHealing>+{regenBonus.Value} <style=cStack>(+{regenBonus.Value} per stack)</style> health regeneration</style>. While at or under <style=cIsHealth>50% health</style>, effects are <style=cIsUtility>doubled</style>.";
        public override string Lore => "<style=cMono>// INTERCEPTED TRANSMISSIONS FROM KOPRULU QUADRANT, SECTOR 19 //</style>\n\n''Looks like you've used this here Stimpack one too many times. Save it for when you REALLY need that extra kick.''\n\n''But sir... I need to heal up before the next fight.''\n\n''Nonsense. You will always recover slowly, no matter the situation. But sometimes, you'll fight enemies so strong that the healing isn't enough to offset their damage. That's where the secondary use of your Stimpack becomes clear: the adrenaline, the speed.''\n\n''I don't understand sir.''\n\n''Big strong foes are slow and lumber about. With a speed advantage, you can easily avoid their fire and come out unscathed. Outsmart your foes, and you will always come out victorious.''\n\n<style=cMono>// END OF TRANSMISSION //</style>";

        public override ItemTier Tier => ItemTier.Tier1;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] {ItemTag.Healing};
        public override bool AIBlackList => false;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("WornOutStimpack.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("WornOutStimpack.prefab");

        public override float minViewport => 1f;
        public override float maxViewport => 3f;

        public override BuffDef ItemBuffDef => BuffList.Find(x => x.Name == "StimpackHealStrong").buffDef;

        private ConfigEntry<float> movementBonus;
        private ConfigEntry<float> regenBonus;

        public override void SetupHooks()
        {
            GlobalEventManager.onServerDamageDealt += (damageReport) =>
            {
                var self = damageReport.victimBody;
                if (!self || !self.inventory) { return; }

                var count = self.inventory.GetItemCount(itemDef);
                if (count <= 0) { return; }

                if (self.healthComponent.health >= self.healthComponent.fullHealth * 0.5f) { return; }

                // Add buff for 5 second duration. If buff already exists, refresh the duration.
                if (self.GetBuffCount(ItemBuffDef) <= 0)
                {
                    self.AddTimedBuff(ItemBuffDef, 5f);
                }
                else
                {
                    self.ClearTimedBuffs(ItemBuffDef);
                    self.AddTimedBuff(ItemBuffDef, 5f);
                }
            };

            // If an entity receives healing, has the buff, and is currently under the low health threshold, the buff
            // is applied or refreshed.
            On.RoR2.HealthComponent.Heal += (orig, self, amount, mask, regen) =>
            {
                if(!self) { return orig(self, amount, mask, regen); }

                var body = self.body;
                if (!body || !body.inventory) { return orig(self, amount, mask, regen); }

                var count = body.inventory.GetItemCount(itemDef);
                if (count <= 0) { return orig(self, amount, mask, regen); }

                if (body.healthComponent.health >= body.healthComponent.fullHealth * 0.5f) { return orig(self, amount, mask, regen); }

                if (body.GetBuffCount(ItemBuffDef) <= 0)
                {
                    body.AddTimedBuff(ItemBuffDef, 5f);
                }
                else
                {
                    body.ClearTimedBuffs(ItemBuffDef);
                    body.AddTimedBuff(ItemBuffDef, 5f);
                }

                return orig(self, amount, mask, regen);
            };


            // Item increases movement speed by 10% and health regen by 0.5 per stack. (Base values)
            R2API.RecalculateStatsAPI.GetStatCoefficients += (self, args) =>
            {
                if (!self || !self.inventory) { return; }

                var count = self.inventory.GetItemCount(itemDef);
                if (count <= 0) { return; }

                float moveBuff = movementBonus.Value * count;
                float regenBuff = regenBonus.Value * count;

                // Double bonuses if the user is under the low health threshold.
                if (self.HasBuff(ItemBuffDef))
                {
                    moveBuff *= 2;
                    regenBuff *= 2;
                }

                args.moveSpeedMultAdd += moveBuff;
                args.baseRegenAdd += regenBuff;
            };
        }

        public override void AddConfigOptions()
        {
            movementBonus = configFile.Bind("Worn-Out_Stimpack Config", "movementBonus", 0.1f, "The movement speed bonus granted by this item. (0.1 = +10%).");
            regenBonus = configFile.Bind("Worn-Out_Stimpack Config", "regenBonus", 0.5f, "The health regeneration bonus granted by this item.");
        }
    }
}