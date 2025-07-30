using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Diagnostics;
using UnityEngine.UIElements;
using static RigsArsenal.RigsArsenal;

namespace RigsArsenal.Items
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
        public override string Description => $"Killing an elite enemy rewards <style=cIsDamage>+{multiplier.Value * goldIncrease * 100f}%</style><style=cStack> (+{multiplier.Value * goldIncrease * 100f}% per stack)</style> increased <style=cIsDamage>gold.</style>";
        public override string Lore => "<style=cMono>// ARTIFACT ANALYSIS NOTES //</style>\n\nNAME: Sheriff Badge.\n\nSIZE: Approximately 12cm x 12cm x 2cm.\n\nWEIGHT: 275g.\n\nMATERIAL: Gold, Rubber.\n\nINVESTIGATOR'S NOTES: Artifact's front shows clear signs of wear and tear. The letters 'J R' has been scribed on the back, perhaps the initials of the former wearer. Under the initials are 5 circular icons scribed in a line, 3 of which crossed out, possibly targets to the former wearer. Further examination of the icons are required. Artifact has been cleared for further forensic and DNA testing.\n\n<style=cMono>// END OF NOTES //";

        public override ItemTier Tier => ItemTier.Tier2;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Utility };
        public override bool AIBlackList => true;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("BountyHunterBadge.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("BountyHunterBadge.prefab");

        public override float minViewport => 1f;
        public override float maxViewport => 2.5f;

        ConfigEntry<float> multiplier;

        private float goldIncrease = 0.25f; // 20% per stack.

        public override void SetupHooks()
        {
            // Give the player more gold when they kill an elite enemy.
            GlobalEventManager.onCharacterDeathGlobal += (DamageInfo) =>
            {
                var victim = DamageInfo.victimBody;
                var player = DamageInfo.attackerBody;

                if (victim == null || player == null || player.inventory == null || !player.isPlayerControlled) { return; }

                var count = player.inventory.GetItemCount(itemDef);

                if (count <= 0) { return; }

                if (victim.isElite)
                {
                    var fractionalBit = 1 / (1 + count * goldIncrease); // Hyperbolic scaling, approaches ~100% of enemy gold value (Base).
                    var increasedGold = Mathf.FloorToInt((1 - fractionalBit) * victim.master.money * multiplier.Value); // Rounded down to the nearest whole number.

                    player.master.GiveMoney((uint)increasedGold);
                }
            };
        }

        public override void AddConfigOptions()
        {
            multiplier = configFile.Bind("Bounty_Hunters_Badge Config", "multiplier", 1f, "Scales the gold per stack and hyperbolic approach limit of the item (1.0 = +20% per stack, approaching +100%)");
        }
    }
}