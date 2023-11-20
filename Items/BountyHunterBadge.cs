using System.Runtime.CompilerServices;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Diagnostics;
using UnityEngine.UIElements;

namespace MoreItems.Items
{
    /// <summary>
    /// Bounty Hunter's Badge - T2 (Uncommon) Item
    /// <para>Killing an elite enemy rewards more gold. More stacks increases the gold reward.</para>
    /// <para>Enemies are unable to get this item.</para>
    /// </summary>
    public class BountyHunterBadge : Item
    {
        public override string Name => "BountyHunterBadge";
        public override string NameToken => "Bounty Hunter's Badge";
        public override string PickupToken => "Increased gold from killing elite enemies.";
        public override string Description => "Killing an elite enemy rewards <style=cIsUtility>25%</style><style=cStack> (+25% per stack)</style> increased gold.";
        public override string Lore => "It's high noon...";

        public override ItemTier Tier => ItemTier.Tier2;

        public override bool CanRemove => true;
        public override bool AIBlackList => false;

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
                    var increasedGold = victim.master.money * count / 4; // +25% total enemy value per stack, linear.
                    player.master.GiveMoney((uint)increasedGold);

                    DebugLog.Log($"Killed an elite; gained {increasedGold} gold.");
                    DebugLog.Log($"Enemy Gold Value: {victim.master.money}");
                    DebugLog.Log($"Total gold value: {victim.master.money + increasedGold}");
                }
            };
        }
    }
}