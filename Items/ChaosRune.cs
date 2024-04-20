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
        public override string PickupToken => "Chance to inflict additional damaging debuffs when applying any debuff.";
        public override string Description => "";
        public override string Lore => "";

        public override ItemTier Tier => ItemTier.Tier3;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Utility };
        public override bool AIBlackList => false;

        public override Sprite Icon => null;
        public override GameObject Model => null;

        // todo: Get rid of addBuff hook as the OnDotsStackAddedServer hook modifies the buffs/debuffs.
        //       A way to identify the attacker and inspect their item count is required too:
        //          - The DotStack class has an attacker object property which may expose the attacker's characterbody component, and so their inventory.
        
        // Note: The OnDotsStackAddedServer hook can also be used to modify (de)buff durations, damage, damage type, etc, which can be handy for some other item ideas.
        public override void SetupHooks()
        {
            On.RoR2.DotController.InflictDot_refInflictDotInfo += (On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo inflictDotInfo) =>
            {
                orig(ref inflictDotInfo);

                var attacker = inflictDotInfo.attackerObject.GetComponent<CharacterBody>();
                var victim = inflictDotInfo.victimObject.GetComponent<CharacterBody>();
                if (!attacker || !victim) { return; }

                var count = attacker.inventory.GetItemCount(itemDef);
                if(count <= 0) { return; }

                var roll = 100;
                var maxSuccessfulRolls = Mathf.Max(2, Mathf.Floor(count * 0.34f)); // 2 successful rolls minimum, or 1 additional successful roll per 3 stacks
                DebugLog.Log($"Up to {maxSuccessfulRolls} successful roles allowed. Total roll count: {count}.");                                                                   // (With many stacks of this item it starts to lag and be more unstable).
                var currentSuccessfulRolls = 0;
                int[] allowedDots = new int[] { 0, 1, 5, 8 }; // Bleed, Burn, Blight (Acrid), Fracture (Collapse).
                                                              // Numbers are just the dot values on the DotController enum, only the order matters for the switch statement.
                
                //todo: test for infinite loop/proc chain crash is gone, test burn + stronger burn interaction, blight and collapse. reset roll value to 20 when done.
                for(int i = 0; i < count; i++)
                {
                    if(currentSuccessfulRolls >= maxSuccessfulRolls) { break; }

                    if (Util.CheckRoll(roll, attacker.master))
                    {
                        DebugLog.Log("Chaos Rune: Successful roll for additional debuff.");
                        var DotIndex = Random.Range(0, allowedDots.Length);

                        // Debuff info is assembled before the debuff is applied, called during the "OnHitEnemy" method. As each debuff has it's own damage and 
                        // duration among other stats, this basically replicates this process of building debuffs.
                        //
                        // Duration of bleed and blight scales off of the attack's proc coefficient. As this isn't exposed here, a flat multiplier is used that
                        // tries to be in-line with the average duration.
                        //
                        // Because this item can inflict lots of debuffss incredibly quickly, the duration of bleed, blight and burn are reduced slightly from an average
                        // value to balance the item, and their damage is reduced by 1/4. The damage of collapse is only reduced by 1/4, duration is unaffected.
                        switch(DotIndex)
                        {
                            /*
                            case 0: // Bleed
                                DebugLog.Log($"Inflicting bleed.");
                                DotController.InflictDot(inflictDotInfo.victimObject, inflictDotInfo.attackerObject, DotController.DotIndex.Bleed, 1.5f, 0.25f);
                                break;
                                
                            case 1: // Burn - As it's damage scales off of the entity's damage value, its damage can be calculated accurately.
                                DebugLog.Log($"Inflicting burn.");
                                InflictDotInfo burnDot = new InflictDotInfo()
                                {
                                    attackerObject = inflictDotInfo.attackerObject,
                                    victimObject = inflictDotInfo.victimObject,
                                    totalDamage = attacker.damage * 0.125f, // Normally it is damage * 0.5f, but it has been scaled down by 1/4 to balance the item.
                                    damageMultiplier = 1f,                  // Reducing the damage by 1/4 seems to reduce its duration by 1/4 and retain the same dps (requires more testing).
                                    dotIndex = DotController.DotIndex.Burn
                                };
                                StrengthenBurnUtils.CheckDotForUpgrade(attacker.inventory, ref burnDot); // Upgrades burn to stronger burn if the entity has any ignition tanks.
                                DotController.InflictDot(ref burnDot);
                                break;

                            case 2: // Blight
                                DebugLog.Log("$Inflicting blight.");
                                DotController.InflictDot(inflictDotInfo.victimObject, inflictDotInfo.attackerObject, DotController.DotIndex.Blight, 2.5f, 0.25f);
                                break;

                            case 4: // Collapse
                                DebugLog.Log("$Inflicting collapse.");
                                DotController.DotDef collapseDef = DotController.GetDotDef(DotController.DotIndex.Fracture);
                                DotController.InflictDot(inflictDotInfo.victimObject, inflictDotInfo.attackerObject, DotController.DotIndex.Fracture, collapseDef.interval, 0.25f);
                                break;
                            */
                            default:
                                DebugLog.Log($"Inflicting burn.");
                                InflictDotInfo burnDot = new InflictDotInfo()
                                {
                                    attackerObject = inflictDotInfo.attackerObject,
                                    victimObject = inflictDotInfo.victimObject,
                                    totalDamage = attacker.damage * 0.125f, // Normally it is damage * 0.5f, but it has been scaled down by 1/4 to balance the item.
                                    damageMultiplier = 1f,                  // Reducing the damage by 1/4 seems to reduce its duration by 1/4 and retain the same dps (requires more testing).
                                    dotIndex = DotController.DotIndex.Burn
                                };
                                StrengthenBurnUtils.CheckDotForUpgrade(attacker.inventory, ref burnDot); // Upgrades burn to stronger burn if the entity has any ignition tanks.
                                DotController.InflictDot(ref burnDot);
                                break;
                        }
                        currentSuccessfulRolls++;
                    }
                }
            };
        }
    }
}