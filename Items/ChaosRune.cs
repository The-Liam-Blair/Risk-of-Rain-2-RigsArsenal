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
    /// Chaos Rune - T3 (Legendary) Item
    /// <para>When applying a debuff to an entity, chance to apply a random damaging debuff as well.</para>
    /// <para>Stacking increases the number of rolls, increasing overall chance and number of debuffs that can be applied at once.</para>
    /// <para>The damage of the debuff scales off of the attack that caused it.</para>
    /// <para>Enemies can get this item, though only ones that can apply debuffs can make use of it.</para>
    /// </summary>
    public class ChaosRune : Item
    {
        public override string Name => "Chaos Rune";
        public override string NameToken => "CHAOSRUNE";
        public override string PickupToken => "Chance to inflict additional damaging debuffs when applying any damaging debuff.";
        public override string Description => "When applying a damaging debuff to an enemy, there is a <style=cIsDamage>33% chance</style><style=cStack> (+1 roll per stack)</style> to apply <style=cIsHealth>additional damaging debuffs</style> up to 1 <style=cStack>(+1 per 2 stacks)</style> more.";
        public override string Lore => "";

        public override ItemTier Tier => ItemTier.Tier3;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Damage };
        public override bool AIBlackList => false; // Even though the AI could get this item, its only going to be useful if the enemy can inflict damaging DOTs
                                                   // naturally or is able to with another item.
        
        public override Sprite Icon => null;
        public override GameObject Model => null;

        private bool _hasRun = false;
        private DamageInfo _damageInfo { get; set; }

        public override void SetupHooks()
        {
            On.RoR2.DotController.InflictDot_refInflictDotInfo += (On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo inflictDotInfo) =>
            {
                orig(ref inflictDotInfo);

                if(_hasRun) { return; }

                if(!inflictDotInfo.attackerObject || !inflictDotInfo.victimObject) { return; }

                var attacker = inflictDotInfo.attackerObject.GetComponent<CharacterBody>();
                var victim = inflictDotInfo.victimObject.GetComponent<CharacterBody>();

                if(!attacker.inventory) { return; }

                var count = attacker.inventory.GetItemCount(itemDef);
                if(count <= 0) { return; }

                var maxSuccessfulRolls = 1 + Mathf.Floor(count * 0.5f); // Up to 1 successful roll, and another per 2 stacks.
                var currentSucessfulRolls = 0;
                
                var roll = 33; // 1/3 chance of a successful roll per stack.
                
                for(int i = 0; i < count; i++)
                {
                    if(currentSucessfulRolls >= maxSuccessfulRolls) { break; }

                    if (Util.CheckRoll(roll, attacker.master))
                    {
                        _hasRun = true;
                        currentSucessfulRolls++;

                        int DotIndex = Random.Range(0, 3); // 4 DOTs: Bleed, Burn (Including ignition tank upgraded burn), Blight and Collapse.

                        switch (DotIndex)
                        {
                            case 0: // Bleed
                                DotController.InflictDot(inflictDotInfo.victimObject, inflictDotInfo.attackerObject, DotController.DotIndex.Bleed, 3f * _damageInfo.procCoefficient, 1f);
                                break;

                            case 1: // Burn
                                InflictDotInfo burnDot = new InflictDotInfo()
                                {
                                    attackerObject = inflictDotInfo.attackerObject,
                                    victimObject = inflictDotInfo.victimObject,
                                    totalDamage = attacker.damage * 0.5f,
                                    damageMultiplier = 1f,
                                    dotIndex = DotController.DotIndex.Burn
                                };
                                StrengthenBurnUtils.CheckDotForUpgrade(attacker.inventory, ref burnDot); // Upgrades burn to stronger burn if the entity has any ignition tanks.
                                DotController.InflictDot(ref burnDot);
                                break;

                            case 2: // Blight
                                DotController.InflictDot(inflictDotInfo.victimObject, inflictDotInfo.attackerObject, DotController.DotIndex.Blight, 5f * _damageInfo.procCoefficient, 1f);
                                break;

                            case 3: // Collapse
                                DotController.DotDef collapseDef = DotController.GetDotDef(DotController.DotIndex.Fracture);
                                DotController.InflictDot(inflictDotInfo.victimObject, inflictDotInfo.attackerObject, DotController.DotIndex.Fracture, collapseDef.interval, 1f);
                                break;
                        }
                    }
                }
            };

            On.RoR2.GlobalEventManager.OnHitEnemy += (orig, self, damageInfo, victim) =>
            {
                _damageInfo = damageInfo; // Store damage info for possible later use to get the attack's proc coefficient.
                _hasRun = false; // Reset flag for triggering this item.
                orig(self, damageInfo, victim);
            };
        }
    }
}