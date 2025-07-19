using RigsArsenal.Buffs;
using R2API;
using Rewired.ComponentControls.Data;
using Rewired.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static RigsArsenal.RigsArsenal;
using BepInEx.Configuration;

namespace RigsArsenal.Equipments
{
    public class SanguineShieldBattery : Equipment
    {
        public override string Name => "Sanguine Shield Battery";

        public override string NameToken => "SANGUINESHIELDBATTERY";

        public override string PickupToken => "Temporarily convert health into barrier.";

        public override string Description => $"Convert {percentHealthDrain.Value * procsPerSecond.Value}% <style=cIsHealth>health</style> into {percentBarrierGain.Value * procsPerSecond.Value}% <style=cIsDamage>barrier</style> per second for {procCount.Value / procsPerSecond.Value} seconds. Barrier drain rate is " + (barrierDecayRateMultiplier.Value >= 0 ? "reduced" : "increased") + $" by <style=cIsDamage>{Mathf.Abs(barrierDecayRateMultiplier.Value) * 100f}%</style> during usage. Effect ends prematurely if <style=cIsHealth>health drops below 5%</style> of your maximum health.";
        public override string Lore => "Shipment: Modified Shield Battery.\nDestination: Ruthsworld Lab, Mars.\nInitial Observation Notes:\n\nStandard-issue exosuit shield battery with multiple bloodstone shards forcefully inserted inside. No longer operable with electricity; the bloodstone's energy is the new power source. Like all bloodstone, it generates and radiates power when in the presence of the dead or dying, said to sap their souls. Prime opportunity to examine the inner workings of these rare crystals. Keep in an enclosed environment at all times.";

        public override bool isLunar => false;

        public override float cooldown => equipCooldown.Value;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("SanguineShieldBattery.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("SanguineShieldBattery.prefab");

        public override float minViewport => 1f;
        public override float maxViewport => 1.5f;

        private ConfigEntry<int> equipCooldown;
        private ConfigEntry<int> percentHealthDrain;
        private ConfigEntry<int> percentBarrierGain;
        private ConfigEntry<int> procCount;
        private ConfigEntry<int> procsPerSecond;
        private ConfigEntry<float> barrierDecayRateMultiplier;

        public override BuffDef EquipmentBuffDef => BuffList.Find(x => x.Name == "SanguineShieldBatteryDuration").buffDef;

        private float intervalTimer = 0f;
        private int intervalCount = 0;

        public override bool UseEquipment(EquipmentSlot slot)
        {
            // On equipment activation, add 20 intervals where the equipment's effects will apply, each with a timer of 0.25s. (Default values).
            intervalCount = procCount.Value;
            intervalTimer = 1f / procsPerSecond.Value;

            var body = slot.characterBody;

            // Give the equipment's buff to the entity.
            // Note that the buff doesn't do anything itself, its just a timer to show the player how long the effect lasts.
            // Duration is calculated as procCount / procsPerSecond.
            if (body.GetBuffCount(EquipmentBuffDef) <= 0)
            {
                body.AddTimedBuff(EquipmentBuffDef, intervalCount / procsPerSecond.Value);
            }
            else
            {
                body.ClearTimedBuffs(EquipmentBuffDef);
                body.AddTimedBuff(EquipmentBuffDef, intervalCount / procsPerSecond.Value);
            }

            return true;
        }

        public override void SetupHooks()
        {

            On.RoR2.Run.FixedUpdate += (orig, self) =>
            {
                orig(self);

                // If the buff is active and the timer has elapsed for 0.25 seconds...
                if (intervalCount > 0 && intervalTimer <= 0f)
                {
                    // Decrement the interval count and reset the timer.
                    intervalCount--;
                    intervalTimer = 1f / procsPerSecond.Value;

                    var body = EquipmentSlot.characterBody;
                    if (!body) { return; }

                    // (Re)calculate the player's max health.
                    float maxHealth = body.healthComponent.fullHealth;

                    // Values scaled down by 0.01 as the config input accepts a percentage value and this converts it to a decimal.
                    body.healthComponent.health -= maxHealth * percentHealthDrain.Value * 0.01f; // Default 2% health drain.
                    body.healthComponent.AddBarrier(maxHealth * percentBarrierGain.Value * 0.01f); // Default 3% barrier gain.

                    body.healthComponent.TakeDamage(new DamageInfo // The damage does nothing but used to trigger the red damage flash visual effect per interval.
                    {
                        damage = 0f,
                        position = body.corePosition,
                        force = Vector3.zero,
                        crit = false,
                        damageType = DamageType.NonLethal,
                        procCoefficient = 0f,
                    });

                    // If the player's health drops below 5% of their max health, end the effect.
                    // Forcefully call recalculate stats afterwards to reset the barrier decay rate to its normal value.
                    if (body.healthComponent.health <= maxHealth * 0.05f)
                    {
                        intervalCount = 0;
                        body.ClearTimedBuffs(EquipmentBuffDef);
                        body.RecalculateStats();
                    }
                }

                // Allow the timer to run if the buff is active.
                if (intervalCount > 0) { intervalTimer -= Time.fixedDeltaTime; }
            };

            // Halve the barrier decay rate while the buff is active.
            On.RoR2.CharacterBody.RecalculateStats += (orig, self) =>
            {
                orig(self);

                if (self.HasBuff(EquipmentBuffDef))
                {
                    EquipmentSlot.characterBody.barrierDecayRate *= barrierDecayRateMultiplier.Value;
                }
            };
        }

        public override void AddConfigOptions()
        {
            equipCooldown = configFile.Bind("Sanguine_Shield_Battery Config", "equipCooldown", 20, "Cooldown for this equipment.");
            percentHealthDrain = configFile.Bind("Sanguine_Shield_Battery Config", "percentHealthDrain", 2, "Percent health drained per proc.");
            percentBarrierGain = configFile.Bind("Sanguine_Shield_Battery Config", "percentBarrierGain", 3, "Percent barrier gained per proc.");
            procCount = configFile.Bind("Sanguine_Shield_Battery Config", "procCount", 20, "Total number of instances where this item will convert health into barrier.");
            procsPerSecond = configFile.Bind("Sanguine_Shield_Battery Config", "procsPerSecond", 4, "Number of times per second a proc will occur. Total equipment duration is equal to procCount / procsPerSecond.");
            barrierDecayRateMultiplier = configFile.Bind("Sanguine_Shield_Battery Config", "barrierDecayRateMultiplier", 0.5f, "Alters barrier decay while the equipment is active. 0.5 = 50% reduced drain rate. Negative values increase drain rate further.");
        }
    }
}