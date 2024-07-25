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
        public override string Description => "Killing an elite enemy rewards <style=cIsDamage>+25%</style><style=cStack> (+20% per stack)</style> increased <style=cIsDamage>gold.</style>";
        public override string Lore => "<style=cMono>// ARTIFACT ANALYSIS NOTES //</style>\n\nNAME: Sheriff Badge.\n\nSIZE: Approximately 12cm x 12cm x 2cm.\n\nWEIGHT: 275g.\n\nMATERIAL: Gold, Rubber.\n\nINVESTIGATOR'S NOTES: Artifact's front shows clear signs of wear and tear. The letters 'J R' has been scribed on the back, perhaps the initials of the former wearer. Under the initials are 5 circular icons scribed in a line, 3 of which crossed out, possibly targets to the former wearer. Further examination of the icons are required. Artifact has been cleared for further forensic and DNA testing.\n\n<style=cMono>// END OF NOTES //";

        public override ItemTier Tier => ItemTier.Tier2;

        public override bool CanRemove => true;

        public override ItemTag[] Tags => new ItemTag[] { ItemTag.Utility };
        public override bool AIBlackList => true;

        public override Sprite Icon => MainAssets.LoadAsset<Sprite>("BountyHunterBadge.png");
        public override GameObject Model => MainAssets.LoadAsset<GameObject>("BountyHunterBadge.prefab");

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
                    var fractionalBit = 1 / (1 + count * 0.25f); // Hyperbolic scaling, approaches ~100% of enemy gold value.
                    var increasedGold = Mathf.FloorToInt((1 - fractionalBit) * victim.master.money); // Rounded down to the nearest whole number.

                    player.master.GiveMoney((uint)increasedGold);
                }
            };
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();

            GameObject display = MainAssets.LoadAsset<GameObject>("BountyHunterBadge.prefab");

            var itemDisplay = display.AddComponent<ItemDisplay>();
            itemDisplay.rendererInfos = ItemDisplaySetup(display);

            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "Chest",
                    localPos = new Vector3(0.10743F, 0.30175F, 0.19213F),
                    localAngles = new Vector3(0.17714F, -0.00021F, 0.0013F),
                    localScale = new Vector3(0.1F, 0.1F, 0.1F)
                }
            });

            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "Chest",
                    localPos = new Vector3(0.07504F, 0.17202F, 0.15712F),
                    localAngles = new Vector3(2.56275F, 14.52645F, 358.1089F),
                    localScale = new Vector3(0.1F, 0.1F, 0.1F)
                }
            });

            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "Chest",
                    localPos = new Vector3(-0.05685F, 0.26051F, 0.16124F),
                    localAngles = new Vector3(6.70086F, 355.7871F, 6.15939F),
                    localScale = new Vector3(0.1F, 0.1F, 0.1F)
                }
            });

            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "Chest",
                    localPos = new Vector3(0.72756F, 1.54271F, 2.53721F),
                    localAngles = new Vector3(305.5826F, 38.15262F, 355.9051F),
                    localScale = new Vector3(0.75F, 0.75F, 0.75F)
                }
            });

            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "Chest",
                    localPos = new Vector3(0.13421F, 0.11844F, 0.26207F),
                    localAngles = new Vector3(46.96029F, 21.93491F, 11.08918F),
                    localScale = new Vector3(0.15F, 0.15F, 0.15F)
                }
            });

            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "Chest",
                    localPos = new Vector3(-0.0862F, 0.16567F, 0.13002F),
                    localAngles = new Vector3(358.0246F, 344.458F, 359.4603F),
                    localScale = new Vector3(0.1F, 0.1F, 0.1F)
                }
            });

            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "Chest",
                    localPos = new Vector3(-0.10726F, 0.19529F, 0.17658F),
                    localAngles = new Vector3(14.99381F, 326.543F, 331.1239F),
                    localScale = new Vector3(0.125F, 0.125F, 0.125F)
                }
            });

            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "Base",
                    localPos = new Vector3(0.24581F, 0.90731F, 0.18949F),
                    localAngles = new Vector3(318.722F, 19.27121F, 329.7198F),
                    localScale = new Vector3(0.2F, 0.2F, 0.2F)
                }
            });

            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "MechBase",
                    localPos = new Vector3(-0.21185F, 0.21048F, 0.45862F),
                    localAngles = new Vector3(337.298F, 359.106F, 181.9463F),
                    localScale = new Vector3(0.125F, 0.125F, 0.125F)
                }
            });

            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "Chest",
                    localPos = new Vector3(-0.96572F, 0.80718F, -2.2202F),
                    localAngles = new Vector3(19.39222F, 193.9269F, 3.58492F),
                    localScale = new Vector3(1F, 1F, 1F)
                }
            });

            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "Chest",
                    localPos = new Vector3(-0.11515F, 0.22685F, 0.18258F),
                    localAngles = new Vector3(13.17887F, 344.5229F, 355.9233F),
                    localScale = new Vector3(0.125F, 0.125F, 0.125F)
                }
            });

            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "Chest",
                    localPos = new Vector3(-0.00939F, 0.12427F, 0.12764F),
                    localAngles = new Vector3(0F, 0F, 0F),
                    localScale = new Vector3(0.1F, 0.1F, 0.1F)
                }
            });

            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = display,
                    childName = "Chest",
                    localPos = new Vector3(-0.07594F, 0.0512F, 0.20567F),
                    localAngles = new Vector3(4.57003F, 30.82439F, 89.66632F),
                    localScale = new Vector3(0.1F, 0.1F, 0.1F)
                }
            });

            return rules;
        }
    }
}