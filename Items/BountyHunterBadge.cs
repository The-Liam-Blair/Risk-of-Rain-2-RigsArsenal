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
    /// Bounty Hunter's Badge - T2 (Uncommon) Item
    /// <para>Killing an elite enemy rewards more gold. More stacks increases the gold reward.</para>
    /// <para>Enemies are unable to get this item.</para>
    /// </summary>
    public class BountyHunterBadge : Item
    {
        public override string Name => "Bounty Hunter's Badge";
        public override string NameToken => "BOUNTYHUNTERBADGE";
        public override string PickupToken => "Increased gold from killing elite enemies.";
        public override string Description => "Killing an elite enemy rewards <style=cIsDamage>+25%</style><style=cStack> (+20% per stack)</style> increased <style=cIsDamage>gold.</style>.";
        public override string Lore => "<style=cMono>// ARTIFACT ANALYSIS NOTES //</style>\n\n NAME: Sheriff Badge.\n\nSIZE: Approximately 12cm x 12cm x 2cm.\n\nWEIGHT: 145g.\n\nMATERIAL: Gold.\n\nINVESTIGATOR'S NOTES: This badge likely belonged to a seasoned mercenary or hit-man, and has likely seen countless battles judging by the rust and scuffs on some of the edges. The letters 'J R' has been scribed on the back, perhaps the initials of the former wearer. Artifact has been cleared for further investigation and DNA testing.\n\n<style=cMono>// END OF NOTES //";

        public override ItemTier Tier => ItemTier.Tier2;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Utility };
        public override bool AIBlackList => true;

        public override Sprite Icon => null;
        public override GameObject Model => null;

        public override void SetupHooks()
        {
            GlobalEventManager.onCharacterDeathGlobal += (DamageInfo) =>
            {
                var victim = DamageInfo.victimBody;
                var player = DamageInfo.attackerBody;

                if (victim == null || player == null || player.inventory == null || !player.isPlayerControlled) { return; }

                var count = player.inventory.GetItemCount(itemDef);

                if (count <= 0) { return; }

                if (victim.isElite)
                {
                    var fractionalBit = 1 / (1 + count * 0.25f); // Hyperbolic scaling, approaches ~100% of enemy gold value.
                    var increasedGold = Mathf.FloorToInt((1 - fractionalBit) * victim.master.money); // Rounded down to the nearest whole number.

                    player.master.GiveMoney((uint)increasedGold);

                    DebugLog.Log($"Killed an elite; gained {increasedGold} gold.");
                    DebugLog.Log($"Enemy Gold Value: {victim.master.money}");
                    DebugLog.Log($"It's fractioning time: {1 - fractionalBit}");
                    DebugLog.Log($"Total gold value: {victim.master.money + increasedGold}");
                }
            };
        }
    }
}