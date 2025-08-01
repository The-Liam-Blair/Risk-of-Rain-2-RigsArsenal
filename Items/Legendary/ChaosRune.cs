﻿using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Diagnostics;
using UnityEngine.UIElements;
using static RigsArsenal.RigsArsenal;
using static RoR2.MasterSpawnSlotController;

namespace RigsArsenal.Items
{
    /// <summary>
    /// Chaos Rune - T3 (Legendary) Item
    /// <para>When applying a damaging debuff to an entity, chance to apply a random damaging debuff as well.</para>
    /// <para>Stacking increases the number of rolls, increasing overall chance and number of debuffs that can be applied at once.</para>
    /// <para>The damage of the debuff scales off of the attack that caused it.</para>
    /// </summary>
    public class ChaosRune : Item
    {
        public override string Name => "Chaos Rune";
        public override string NameToken => "CHAOSRUNE";
        public override string PickupToken => "Chance to inflict additional damaging debuffs when applying any damaging debuff.";
        public override string Description => $"When applying a damaging debuff to an enemy, there is a <style=cIsDamage>{procChance.Value}% chance</style><style=cStack> (+{rollsPerStack.Value} roll(s) per stack)</style> to apply <style=cIsHealth>additional damaging debuffs</style>.";
        public override string Lore => "<style=cMono>// ARTIFACT RECOVERY NOTES: EXCAVATION SITE 165-A34 //</style>\n\nName: Runic Stone Carving\n\nSize: 20cm by 20cm by 3cm\n\nSite Notes: ''Weighty and shimmers a bright red hue. The miner that recovered this artifact was found an hour after contact in tremendous pain, dehydrated and collapsed, still holding onto the artifact. Artifact was additionally glowing incredibly brightly, and is allegedly scalding to the touch for some while bone-chillingly cold to others.\n\nDo NOT handle directly. Do NOT stare into it's glow. Do NOT listen to what it offers. Be not tempted.''\n\n<style=cMono>// END OF NOTES //";

        public override ItemTier Tier => ItemTier.Tier3;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Damage };
        public override bool AIBlackList => true; // Even though the AI could get this item, its only going to be useful if the enemy can inflict damaging DOTs
                                                   // naturally or is able to with another item, so its too niche.
        
        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("ChaosRune.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("ChaosRune.prefab");

        public override float minViewport => 1f;
        public override float maxViewport => 1.8f;

        ConfigEntry<int> procChance;
        ConfigEntry<int> rollsPerStack;


        private bool hasRun = false;
        private DamageInfo damageInfo { get; set; }

        public override void SetupHooks()
        {
            On.RoR2.DotController.InflictDot_refInflictDotInfo += (On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo inflictDotInfo) =>
            {
                orig(ref inflictDotInfo);

                if (hasRun) { return; }

                if (!inflictDotInfo.attackerObject || !inflictDotInfo.victimObject) { return; }

                var attacker = inflictDotInfo.attackerObject.GetComponent<CharacterBody>();
                var victim = inflictDotInfo.victimObject.GetComponent<CharacterBody>();

                if (!attacker.inventory) { return; }

                var count = attacker.inventory.GetItemCount(itemDef);
                if (count <= 0) { return; }

                var procCoefficient = damageInfo.procCoefficient;
                if (procCoefficient <= 0f) {  procCoefficient = 1f; } // For niche items that apply DOTs with a proc coefficient of 0: Gasoline and Umbral Pyre.

                var roll = procChance.Value; // 1/3 chance of a successful roll per stack (With base values).
                
                for(int i = 0; i < rollsPerStack.Value; i++)
                {
                    if (Util.CheckRoll(roll, attacker.master))
                    {
                        hasRun = true;

                        // todo: some custom visual or audio effect maybe to indicate the item has triggered.

                        int DotIndex = UnityEngine.Random.Range(0, 4); // 4 DOTs: Bleed, Burn (Including ignition tank upgraded burn), Blight and Collapse.

                        switch (DotIndex)
                        {
                            case 0: // Bleed
                                InflictDot(attacker, victim, DotController.DotIndex.Bleed, attacker.damage, procCoefficient);
                                break;

                            case 1: // Burn
                                InflictDot(attacker, victim, DotController.DotIndex.Burn, attacker.damage, procCoefficient);
                                break;

                            case 2: // Blight
                                InflictDot(attacker, victim, DotController.DotIndex.Blight, attacker.damage, procCoefficient);
                                break;

                            case 3: // Collapse
                                InflictDot(attacker, victim, DotController.DotIndex.Fracture, attacker.damage, procCoefficient);
                                break;
                        }
                    }
                }
            };

            On.RoR2.GlobalEventManager.OnHitEnemy += (orig, self, DamageInfo, victim) =>
            {
                damageInfo = DamageInfo; // Store damage info for possible later use to get the attack's proc coefficient.
                hasRun = false; // Reset flag for triggering this item.
                orig(self, damageInfo, victim);
            };
        }

        public override void AddConfigOptions()
        {
            procChance = configFile.Bind("Chaos_Rune Config", "procChance", 33, "The chance of the item's effect triggering per item stack on applying a DOT.");
            rollsPerStack = configFile.Bind("Chaos_Rune Config", "rollsPerStack", 1, "Number of times the item will roll on activation per item stack.");
        }
    }
}