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
            // Called when a buff/debuff is given to an entity.
            On.RoR2.CharacterBody.AddBuff_BuffDef += (orig, self, buffDef) =>
            {
                orig(self, buffDef);

                //   var count = self.inventory.GetItemCount(itemDef);
                //   if (count <= 0) { return; }

                if (buffDef != null)
                {
                    DebugLog.Log($"Chaos Rune: Debuff {buffDef.name} applied to {self.name}");
                }
                else
                {
                    DebugLog.Log($"Huh, no debuff applied to {self.name}");
                }
            };

            // Called when a buff/debuff has been assembled and about to be added onto the target.
            On.RoR2.DotController.OnDotStackAddedServer += (orig, self, dotStack) =>
            {
                // For some reason dotstack is interpreted as an object, not as a DotStack class, so its casted into that type so it can be modified.
                DotController.DotStack castedDotStack = dotStack as DotController.DotStack;

                if (castedDotStack != null)
                {
                    castedDotStack.damage *= 5f;
                }

                orig(self, dotStack);
            };
        }
    }
}