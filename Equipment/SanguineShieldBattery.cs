using MoreItems.Buffs;
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
using static MoreItems.MoreItems;

namespace MoreItems.Equipments
{
    public class SanguineShieldBattery : Equipment
    {
        public override string Name => "Sanguine Shield Battery";

        public override string NameToken => "SANGUINESHIELDBATTERY";

        public override string PickupToken => "Temporarily convert health into barrier.";

        public override string Description => "Convert 8% <style=cIsHealth>health</style> into 12% <style=cIsDamage>barrier</style> per second for 5 seconds. Barrier drain rate is <style=cIsDamage>halved</style> during usage. Effect ends prematurely if <style=cIsHealth>health drops below 5%</style> of your maximum health.";
        public override string Lore => "Shipment: Modified Shield Battery.\nDestination: Ruthsworld Lab, Mars.\nInitial Observation Notes:\n\nStandard-issue exosuit shield battery injected with multiple bloodstone shards. No longer operable with electricity; the bloodstone's energy is the new power source. Like all bloodstone, it grows in power when in the presence of the dead or dying, said to sap their souls. Prime opportunity to examine the inner workings of these rare crystals. Keep in an enclosed environment at all times.";

        public override bool isLunar => false;

        public override float cooldown => 20f;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("SanguineShieldBattery.png");

        public override GameObject Model => MainAssets.LoadAsset<GameObject>("SanguineShieldBattery.prefab");

        public override BuffDef EquipmentBuffDef => BuffList.Find(x => x.Name == "SanguineShieldBatteryDuration").buffDef;

        private float intervalTimer = 0f;
        private int intervalCount = 0;

        public override bool UseEquipment(EquipmentSlot slot)
        {
            // On equipment activation, add 20 intervals where the equipment's effects will apply, each with a timer of 0.25s
            // for a total of around 5.3 seconds. (The extra 0.3 is probably from floating point errors or inconsistencies in dt).
            intervalCount = 20;
            intervalTimer = 0.25f;

            var body = slot.characterBody;

            // Give the equipment's buff to the entity.
            // Note that the buff doesn't do anything itself, its just a timer to show the player how long the effect lasts.
            if (body.GetBuffCount(EquipmentBuffDef) <= 0)
            {
                body.AddTimedBuff(EquipmentBuffDef, 5.3f);
            }
            else
            {
                body.ClearTimedBuffs(EquipmentBuffDef);
                body.AddTimedBuff(EquipmentBuffDef, 5.3f);
            }

            return true;
        }

        
        public override void SetupHooks()
        {
            /*
            On.RoR2.Run.Update += (orig, self) =>
            {
                orig(self);

                // If the buff is active and the timer has elapsed for 0.25 seconds...
                if(intervalCount > 0 && intervalTimer <= 0f)
                {
                    // Decrement the interval count and reset the timer.
                    intervalCount--;
                    intervalTimer = 0.25f;

                    var body = EquipmentSlot.characterBody;

                    // (Re)calculate the player's max health.
                    float maxHealth = body.healthComponent.fullHealth;

                    body.healthComponent.health -= maxHealth * 0.02f; // 2% health loss, 4 times per second.
                    body.healthComponent.AddBarrier(maxHealth * 0.03f); // 3% barrier gain, 4 times per second.

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
                if (intervalCount > 0) { intervalTimer -= Time.deltaTime; }
            };

            // Halve the barrier decay rate while the buff is active.
            On.RoR2.CharacterBody.RecalculateStats += (orig, self) =>
            {
                orig(self);

                if (self.HasBuff(EquipmentBuffDef))
                {
                    EquipmentSlot.characterBody.barrierDecayRate *= 0.5f;
                }
            };
            */
        }
    }
}